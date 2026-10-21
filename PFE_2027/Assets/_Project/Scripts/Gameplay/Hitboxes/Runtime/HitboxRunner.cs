using System;
using System.Collections.Generic;
using PFE.Gameplay.Scripts.Animations;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public class HitboxRunner
    {
        private const int MAX_COLLIDERS = 16;
        
        private readonly ActionAnimator actionAnimator;
        private readonly HitboxAnchors anchors;
        private readonly IDamageable owner;         // The attacker itself, never hit by its own attack (can be null)
        private readonly LayerMask hitMask;
        private readonly AttackHitHandler onHit;
        private readonly WindowEnterHandler onWindowEnter;
        
        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];
        private readonly List<IDamageable> alreadyHit = new(MAX_COLLIDERS);
        private Transform[] resolvedAnchors = new Transform[4];
        
        private HitboxData current;
        private int playId = -1;
        private float previousTime;
        private float currentSeconds;       // Real duration of the current action, converts windows into seconds

        public bool IsRunning => current != null;

        public HitboxData Current => current;

        // Flags of every window containing the current time, rebuilt each tick (empty while no attack runs)
        private readonly List<IWindowFlag> activeFlags = new();

        public IReadOnlyList<IWindowFlag> ActiveFlags => activeFlags;

        // True if a flag of this type is active; gives it back to read its values
        public bool Has<TFlag>(out TFlag flag) where TFlag : IWindowFlag
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

        public HitboxRunner(HitboxContext context)
        {
            actionAnimator = context.actionAnimator;
            anchors = context.anchors;
            owner = context.owner;
            hitMask = context.hitMask;
            onHit = context.onHit;
            onWindowEnter = context.onWindowEnter;
        }
        
        public void Begin(HitboxData data)
        {
            playId = actionAnimator.Play(data.AnimationClip, duration: data.PlayDuration);
            if (playId < 0)
            {
                Debug.LogError($"[AttackRunner] Attack '{data.name}' has no animation clip.", data);
                End();
                return;
            }

            current = data;
            currentSeconds = data.PlaySeconds;
            previousTime = -1f;
            alreadyHit.Clear();
            activeFlags.Clear();

            ResolveAnchors(data);
        }

        private void ResolveAnchors(HitboxData data)
        {
            ReadOnlySpan<HitboxWindow> hitboxes = data.Hitboxes;
            if (resolvedAnchors.Length < hitboxes.Length)
                resolvedAnchors = new Transform[hitboxes.Length];   
            
            for (int i = 0; i < hitboxes.Length; i++)
            {
                if (!anchors.TryGet(hitboxes[i].Anchor, out resolvedAnchors[i]))
                    Debug.LogError($"[AttackRunner] '{anchors.name}' has no bone for anchor '{hitboxes[i].Anchor?.name}' (attack '{data.name}').", anchors);
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

            // Rebuilt every tick: a flag only lives while the time is inside its window
            activeFlags.Clear();
            ReadOnlySpan<AdditionalWindow> windows = current.Windows;
            for (int i = 0; i < windows.Length; i++)
            {
                ref readonly AdditionalWindow window = ref windows[i];
                if (window.Flag == null)
                    continue;

                if (window.Window.Contains(time))
                    activeFlags.Add(window.Flag);

                // Opened since the last tick (also catches a window shorter than a frame)
                if (window.Window.Overlaps(previousTime, time) && !window.Window.Contains(previousTime))
                    onWindowEnter?.Invoke(window.Flag, (window.Window.End - window.Window.Start) * currentSeconds);
            }

            ReadOnlySpan<HitboxWindow> hitboxes = current.Hitboxes;
            for (int i = 0; i < hitboxes.Length; i++)
            {
                ref readonly HitboxWindow hitbox = ref hitboxes[i];
                if (hitbox.Window.Overlaps(previousTime, time))
                    Query(in hitbox, resolvedAnchors[i]);
            }

            previousTime = time;
        }

        // Also cancels the animation when the attack is interrupted before its end
        public void End()
        {
            actionAnimator.Stop(playId);
            playId = -1;
            current = null;
            activeFlags.Clear();
        }
        
        // Detection only: finds what the hitbox touches and hands each new target to the owner's callback
        private void Query(in HitboxWindow hitbox, Transform anchor)
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
                onHit?.Invoke(new HitboxHit(target, collider, in hitbox, center));
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
