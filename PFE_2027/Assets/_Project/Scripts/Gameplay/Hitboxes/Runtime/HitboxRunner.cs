using System;
using System.Collections.Generic;
using PFE.Gameplay.Scripts.Animations;
using UnityEngine;

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
        
        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];
        private readonly List<IDamageable> alreadyHit = new(MAX_COLLIDERS);
        private Transform[] resolvedAnchors = new Transform[4];
        
        private HitboxData current;
        private int playId = -1;
        private float previousTime;

        public bool IsRunning => current != null;

        public HitboxData Current => current;

        // Union of the flags of every window containing the current time (None while no attack runs)
        public WindowFlags ActiveFlags { get; private set; }

        // True if at least one of the given flags is active
        public bool Has(WindowFlags flags) => (ActiveFlags & flags) != 0;

        public HitboxRunner(HitboxContext context)
        {
            actionAnimator = context.actionAnimator;
            anchors = context.anchors;
            owner = context.owner;
            hitMask = context.hitMask;
            onHit = context.onHit;
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
            previousTime = -1f;
            alreadyHit.Clear();
            ActiveFlags = WindowFlags.None;

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

            WindowFlags active = WindowFlags.None;
            ReadOnlySpan<AdditionalWindow> windows = current.Windows;
            for (int i = 0; i < windows.Length; i++)
            {
                var attackWindow = windows[i];
                if (attackWindow.Window.Contains(time))
                    active |= attackWindow.Flags;
            }
            ActiveFlags = active;

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
            ActiveFlags = WindowFlags.None;
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
