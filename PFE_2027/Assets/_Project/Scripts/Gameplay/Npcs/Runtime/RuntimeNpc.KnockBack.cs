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
    }
}
