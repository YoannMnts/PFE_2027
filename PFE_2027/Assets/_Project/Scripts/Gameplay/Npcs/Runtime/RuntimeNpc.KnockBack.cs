using PFE.Core.Scripts.Enemy.Attacks;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Knockback driven by an AnimationCurve (distance travelled over time, PushbackData.DistanceInTime),
    // scaled by the strength of the hit. The NPC is moved on the NavMesh, so walls and edges are respected.
    public abstract partial class RuntimeNpc
    {
        private AnimationCurve knockbackCurve;
        private Vector3 knockbackDirection;     // Flat and normalized
        private float knockbackStrength;        // Multiplier applied to the distance of the curve
        private float knockbackDuration;
        private float knockbackElapsed;

        public bool IsKnockedBack { get; private set; }

        // Current sliding velocity (m/s), zero when not knocked back. Used to push what the NPC bumps into.
        public Vector3 KnockbackVelocity { get; private set; }

        // Starts (or restarts) the knockback. Returns its duration, 0 if nothing started.
        protected float StartKnockback(Vector3 direction, float strength, AnimationCurve curve)
        {
            direction.y = 0f;
            if (curve == null || curve.length == 0 || strength <= 0f || direction.sqrMagnitude < 0.0001f)
                return 0f;

            float duration = curve[curve.length - 1].time;
            if (duration <= 0f)
                return 0f;

            knockbackCurve = curve;
            knockbackDirection = direction.normalized;
            knockbackStrength = strength;
            knockbackDuration = duration;
            knockbackElapsed = 0f;
            IsKnockedBack = true;

            return duration;
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

            float deltaTime = Time.deltaTime;
            float previousTime = knockbackElapsed;
            knockbackElapsed = Mathf.Min(knockbackElapsed + deltaTime, knockbackDuration);

            // Distance to cover this frame: the total always ends exactly on the last key of the curve
            float distance = (knockbackCurve.Evaluate(knockbackElapsed) - knockbackCurve.Evaluate(previousTime))
                             * knockbackStrength;
            Vector3 step = knockbackDirection * distance;
            NavMeshAgent.Move(step);

            KnockbackVelocity = deltaTime > 0f ? step / deltaTime : Vector3.zero;

            if (knockbackElapsed >= knockbackDuration)
                StopKnockback();
        }

        private void StopKnockback()
        {
            IsKnockedBack = false;
            KnockbackVelocity = Vector3.zero;
            knockbackCurve = null;
        }

        private const int MAX_REDIRECT_CANDIDATES = 16;

        // Shared by every NPC: the queries run one at a time on the main thread
        private static readonly Collider[] RedirectResults = new Collider[MAX_REDIRECT_CANDIDATES];

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
