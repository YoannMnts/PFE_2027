using PFE.Core.Scripts.Curves;
using PFE.Core.Scripts.Enemy.Attacks;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Knockback driven by an AnimationCurve (PushbackData.ProgressOverTime, see ProgressCurveExtensions).
    // The NPC is moved on the NavMesh, so walls and edges are respected.
    public abstract partial class RuntimeNpc
    {
        private const int MAX_REDIRECT_CANDIDATES = 16;

        // Shared by every NPC: the queries run one at a time on the main thread
        private static readonly Collider[] RedirectResults = new Collider[MAX_REDIRECT_CANDIDATES];

        public bool IsKnockedBack { get; private set; }
        
        // Strength of this NPC's hits: 1 normally (the hitbox decides alone), what is left of its slide while
        // knocked back (full right after being hit, 0 at the end of the curve)
        protected float StrengthScale => IsKnockedBack
            ? knockbackStrength * (1f - Mathf.Clamp01(knockbackCurve.EvaluateProgress(knockbackElapsed)))
            : 1f;

        // Direction used when this NPC and its target overlap: where it slides, or where it faces
        protected Vector3 HitFallbackDirection => IsKnockedBack
            ? knockbackDirection
            : (Mesh != null ? Mesh : transform).forward;
        
        
        private AnimationCurve knockbackCurve;
        private Vector3 knockbackDirection;     // Flat and normalized
        private float knockbackDistance;        // Total distance of this knockback (meters)
        private float knockbackStrength;        // Strength received, passed on to what this NPC bumps into
        private float knockbackDuration;
        private float knockbackElapsed;
        private IRuntimeNpc knockbackSource;

        // The NPC that pushed this one can't be pushed back by it (no ping-pong between two sliding NPCs)
        protected bool IsKnockbackSource(IRuntimeNpc npc) =>
            IsKnockedBack && knockbackSource != null && ReferenceEquals(knockbackSource, npc);

        // Starts (or restarts) the knockback. Returns its duration, 0 if nothing started.
        protected float StartKnockback(in KnockbackRequest request, float distance, AnimationCurve curve)
        {
            float duration = curve.GetProgressDuration();
            if (!request.IsValid || distance <= 0f || duration <= 0f)
                return 0f;

            knockbackCurve = curve;
            knockbackDirection = request.direction;
            knockbackDistance = distance;
            knockbackStrength = request.strength;
            knockbackSource = request.source;
            knockbackDuration = duration;
            knockbackElapsed = 0f;
            IsKnockedBack = true;

            return knockbackDuration;
        }

        private void UpdateKnockback()
        {
            if (!IsKnockedBack)
                return;

            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
            {
                StopKnockback();
                return;
            }

            float previousTime = knockbackElapsed;
            knockbackElapsed = Mathf.Min(knockbackElapsed + Time.deltaTime, knockbackDuration);

            // Part of the curve covered this frame: the total always ends exactly on knockbackDistance
            float fraction = knockbackCurve.EvaluateProgressDelta(previousTime, knockbackElapsed);
            NavMeshAgent.Move(knockbackDirection * (fraction * knockbackDistance));

            if (knockbackElapsed >= knockbackDuration)
                StopKnockback();
        }

        private void StopKnockback()
        {
            IsKnockedBack = false;
            knockbackCurve = null;
            knockbackStrength = 0f;
            knockbackSource = null;
        }
        
        // Aim assist for "knockback into knockback": bends the knockback toward the NPC closest to the axis of a
        // cone around its direction. Returns the original direction when nobody is in the cone.
        protected Vector3 RedirectKnockback(Vector3 direction, KnockbackRedirectData redirect, LayerMask mask)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return direction;

            Vector3 origin = transform.position;
            Vector3 axis = direction.normalized;

            // Only targets inside the cone can beat it; then the highest dot wins (smallest angle to the axis)
            float bestDot = Mathf.Cos(redirect.HalfAngle * Mathf.Deg2Rad);
            Vector3 bestDirection = direction;

            int count = Physics.OverlapSphereNonAlloc(origin, redirect.Range, RedirectResults, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                RuntimeNpc other = RedirectResults[i].GetComponentInParent<RuntimeNpc>();
                if (other == null || other == this)
                    continue;

                Vector3 toOther = other.transform.position - origin;
                toOther.y = 0f;

                float distance = toOther.magnitude;
                if (distance < 0.01f)
                    continue;

                Vector3 toOtherDirection = toOther / distance;
                float dot = Vector3.Dot(axis, toOtherDirection);
                if (dot < bestDot)
                    continue;

                bestDot = dot;
                bestDirection = toOtherDirection;
            }

            return bestDirection;
        }
    }
}
