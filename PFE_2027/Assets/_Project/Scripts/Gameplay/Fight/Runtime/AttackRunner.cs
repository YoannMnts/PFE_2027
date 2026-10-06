using System;
using System.Collections.Generic;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public sealed class AttackRunner
    {
        private const int MAX_COLLIDERS = 16;
        private const float CROSS_FADE_DURATION = 0.05f;
        
        private readonly Animator animator;
        private readonly HitboxAnchors anchors;
        private readonly IDamageable owner;         // The attacker itself, never hit by its own attack (can be null)
        private readonly LayerMask hitMask;
        private readonly AttackHitHandler onHit;
        
        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];
        private readonly List<IDamageable> alreadyHit = new(MAX_COLLIDERS);
        private Transform[] resolvedAnchors = new Transform[4];
        
        private AttackData current;
        private float previousTime;
        private float currentTime;
        private bool hasEntered;

        public bool IsRunning => current != null;

        public AttackData Current => current;

        // Union of the flags of every window containing the current time (None while no attack runs)
        public AttackFlags ActiveFlags { get; private set; }

        // True if at least one of the given flags is active
        public bool Has(AttackFlags flags) => (ActiveFlags & flags) != 0;

        public AttackRunner(Animator animator, HitboxAnchors anchors, IDamageable owner, LayerMask hitMask, AttackHitHandler onHit)
        {
            this.animator = animator;
            this.anchors = anchors;
            this.owner = owner;
            this.hitMask = hitMask;
            this.onHit = onHit;
        }
        
        public void Begin(AttackData data)
        {
            current = data;
            previousTime = -1f;
            currentTime = 0f;
            hasEntered = false;
            alreadyHit.Clear();
            ActiveFlags = AttackFlags.None;

            ResolveAnchors(data);

            animator.CrossFadeInFixedTime(data.StateHash, CROSS_FADE_DURATION, 0, 0f);
        }

        private void ResolveAnchors(AttackData data)
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

            if (!animator.TryGetStateTime(0, current.StateHash, out float time))
            {
                if (hasEntered)
                    End();
                return;
            }

            hasEntered = true;
            currentTime = time;

            AttackFlags active = AttackFlags.None;
            ReadOnlySpan<AttackWindow> windows = current.Windows;
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

        public void End()
        {
            current = null;
            ActiveFlags = AttackFlags.None;
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
                onHit?.Invoke(new AttackHit(target, collider, in hitbox, center));
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