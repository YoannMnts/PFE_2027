using System;
using System.Collections.Generic;
using PFE.Core.Scripts.Fight.Charges;
using PFE.Gameplay.Scripts.Animations;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public class AnimationRunner
    {
        private const int MAX_COLLIDERS = 16;
        
        private readonly ActionAnimator actionAnimator;
        private readonly HitboxAnchors anchors;
        private readonly IDamageable owner;         // The attacker itself, never hit by its own attack (can be null)
        private readonly LayerMask hitMask;
        private readonly HitboxHitHandler defaultHit;
        private readonly WindowEnterHandler onWindowEnter;
        
        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];
        private readonly List<IDamageable> alreadyHit = new(MAX_COLLIDERS);
        private Transform[] resolvedAnchors = new Transform[4];
        
        private AnimationData current;
        private int playId = -1;
        private float previousTime;
        private float currentSeconds;       // Real duration of the current action, converts windows into seconds
        private HitboxHitHandler currentHit;
        private float startTime;            // Time.time when the current action began

        public bool IsRunning => current != null;

        public AnimationData Current => current;

        // Flags active at the current time: the ones without window during the whole action, the timed ones
        // while the time is inside their window. Rebuilt each tick, empty while nothing runs.
        private readonly List<IAnimationFlag> activeFlags = new();

        public IReadOnlyList<IAnimationFlag> ActiveFlags => activeFlags;

        // A charge level (ChargeData) is the current action
        public bool IsCharging => current is ChargeData;

        // Index of the charge level playing (1 = first level), 0 when no charge runs
        public int ChargeLevel { get; private set; }

        // True if a flag of this type is active now; gives it back to read its values
        public bool Has<TFlag>(out TFlag flag) where TFlag : IAnimationFlag
        {
            for (int i = 0; i < activeFlags.Count; i++)
            {
                if (activeFlags[i] is not TFlag compatible)
                    continue;

                flag = compatible;
                return true;
            }

            flag = default;
            return false;
        }

        public AnimationRunner(AnimationContext context)
        {
            actionAnimator = context.actionAnimator;
            anchors = context.anchors;
            owner = context.owner;
            hitMask = context.hitMask;
            defaultHit = context.onHit;
            onWindowEnter = context.onWindowEnter;
        }
        
        public void Begin(AnimationData data, HitboxHitHandler specificAction = null)
        {
            // Any action started from outside cancels the charge in progress
            ResetCharge();
            Play(data, specificAction);
        }

        // Starts the charge of the action if it has a ChargeUp flag (false otherwise, nothing started).
        // Its levels then follow each other on their own (LateTick), ReleaseCharge ends it.
        public bool TryBeginCharge(AnimationData action, HitboxHitHandler specificAction = null)
        {
            if (action == null || !action.TryGetFlag(out ChargeUp chargeUp) || chargeUp.Charge == null)
                return false;

            Begin(chargeUp.Charge, specificAction);
            if (IsCharging)
                ChargeLevel = 1;

            return IsCharging;
        }

        // Ends the current charge. Returns the Release of the level playing (what its animation announces),
        // null if no charge is running (e.g. interrupted by another action) or if that level has no Release
        public AnimationData ReleaseCharge()
        {
            if (current is not ChargeData level)
                return null;

            AnimationData release = level.Release;
            End();
            return release;
        }

        private void Play(AnimationData data, HitboxHitHandler specificAction)
        {
            //DOUANE
            currentHit = specificAction ?? defaultHit;

            //joue l'anim
            bool loop = data.TryGetFlag(out LoopAnimation _);
            playId = actionAnimator.Play(data.AnimationClip, duration: data.PlayDuration, loop: loop);
            
            if (playId < 0)
            {
                Debug.LogError($"[AttackRunner] Attack '{data.name}' has no animation clip.", data);
                End();
                return;
            }

            if (data is ChargeData charge && charge.Next != null && !data.TryGetFlag(out ChargeTime _))
                Debug.LogWarning($"[AttackRunner] Charge '{data.name}' has a Next level but no ChargeTime flag: the charge never moves on.", data);

            current = data;
            currentSeconds = data.PlaySeconds;
            startTime = Time.time;
            previousTime = -1f;
            alreadyHit.Clear();
            activeFlags.Clear();

            // Flags without window are active from the very start, before the first LateTick
            ReadOnlySpan<IAnimationFlag> flags = data.Flags;
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i] != null && flags[i] is not ITimedFlag)
                    activeFlags.Add(flags[i]);
            }

            ResolveAnchors(data);
        }

        // One slot per flag (same index as in Flags), only filled for the Hitbox flags
        private void ResolveAnchors(AnimationData data)
        {
            ReadOnlySpan<IAnimationFlag> flags = data.Flags;
            if (resolvedAnchors.Length < flags.Length)
                resolvedAnchors = new Transform[flags.Length];

            for (int i = 0; i < flags.Length; i++)
            {
                resolvedAnchors[i] = null;
                if (flags[i] is not Hitbox hitbox)
                    continue;

                if (!anchors.TryGet(hitbox.Anchor, out resolvedAnchors[i]))
                    Debug.LogError($"[AttackRunner] '{anchors.name}' has no bone for anchor '{hitbox.Anchor?.name}' (attack '{data.name}').", anchors);
            }
        }
        
        public void LateTick()
        {
            if (current == null)
                return;

            // The clip ended, or another action (dash, pushback, next attack...) took its place
            if (!actionAnimator.TryGetNormalizedTime(playId, out float time))
            {
                End();
                return;
            }

            // Looped (LoopAnimation flag): the windows open again from the start of the clip.
            // A target stays hit once per action (alreadyHit is kept), even across loops.
            if (time < previousTime)
                previousTime = -1f;

            // One pass over the flags: the active ones, the windows opening, the hitboxes crossed since the last tick
            activeFlags.Clear();
            ReadOnlySpan<IAnimationFlag> flags = current.Flags;
            for (int i = 0; i < flags.Length; i++)
            {
                IAnimationFlag flag = flags[i];
                if (flag == null)
                    continue;

                // No window: active during the whole action
                if (flag is not ITimedFlag timed)
                {
                    activeFlags.Add(flag);
                    continue;
                }

                TimeWindow window = timed.Window;
                if (window.Contains(time))
                    activeFlags.Add(flag);

                // Crossed since the last tick (also catches a window shorter than a frame)
                if (!window.Overlaps(previousTime, time))
                    continue;

                if (!window.Contains(previousTime))
                    onWindowEnter?.Invoke(timed, (window.End - window.Start) * currentSeconds);

                if (flag is Hitbox hitbox)
                    Query(in hitbox, resolvedAnchors[i]);
            }

            previousTime = time;
            UpdateCharge();
        }

        // Moves on to the next charge level once the ChargeTime of the current one elapsed
        private void UpdateCharge()
        {
            if (current is not ChargeData level || level.Next == null)
                return;

            if (!level.TryGetFlag(out ChargeTime chargeTime) || Time.time - startTime < chargeTime.Duration)
                return;

            // Not Begin: the charge goes on with the next level
            Play(level.Next, currentHit);
            ChargeLevel++;
        }

        // Also cancels the animation when the attack is interrupted before its end
        public void End()
        {
            //peut peut etre passer en null
            currentHit = defaultHit;
            
            actionAnimator.Stop(playId);
            playId = -1;
            current = null;
            activeFlags.Clear();
            ResetCharge();
        }

        private void ResetCharge() => ChargeLevel = 0;
        
        // Detection only: finds what the hitbox touches and hands each new target to the owner's callback
        private void Query(in Hitbox hitbox, Transform anchor)
        {
            if (anchor == null)
                return;

            HitboxMath.GetPose(anchor, hitbox, out Vector3 center, out Quaternion rotation);
            
            int count = hitbox.Shape switch
            {
                HitboxShape.Box => Physics.OverlapBoxNonAlloc(center, hitbox.Size * 0.5f, overlapResults, rotation, hitMask, QueryTriggerInteraction.Collide),
                HitboxShape.Sphere => Physics.OverlapSphereNonAlloc(center, hitbox.Size.x, overlapResults, hitMask, QueryTriggerInteraction.Collide),
                HitboxShape.Capsule => OverlapCapsule(center, rotation, hitbox.Size),
                _ => 0,
            };

            for (int i = 0; i < count; i++)
            {
                Collider collider = overlapResults[i];
                IDamageable target = collider.GetComponentInParent<IDamageable>();
                if (target == null || ReferenceEquals(target, owner) || alreadyHit.Contains(target))
                    continue;

                alreadyHit.Add(target);
                currentHit?.Invoke(new HitboxHit(target, collider, in hitbox, center));
            }
        }

        private int OverlapCapsule(Vector3 center, Quaternion rotation, Vector3 size)
        {
            HitboxMath.GetCapsule(center, rotation, size, 
                out Vector3 point0, out Vector3 point1, out var radius);
            
            return Physics.OverlapCapsuleNonAlloc
                (point0, point1, radius, overlapResults, hitMask, QueryTriggerInteraction.Collide);
        }
    }
}
