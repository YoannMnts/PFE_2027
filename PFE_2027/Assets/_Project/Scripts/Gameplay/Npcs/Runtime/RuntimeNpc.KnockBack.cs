using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    public abstract partial class RuntimeNpc
    {
        private const float KNOCKBACK_STOP_SPEED = 0.05f;

        public Vector3 KnockbackVelocity { get; protected set; }
        public float KnockbackDamping { get; protected set; }
        
        public bool IsKnockedBack => KnockbackVelocity.sqrMagnitude > KNOCKBACK_STOP_SPEED * KNOCKBACK_STOP_SPEED;

        private void UpdateKnockback()
        {
            if (!IsKnockedBack)
                return;

            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
            {
                KnockbackVelocity = Vector3.zero;
                return;
            }

            NavMeshAgent.Move(KnockbackVelocity * Time.deltaTime);
            KnockbackVelocity *= Mathf.Exp(-KnockbackDamping * Time.deltaTime);

            if (!IsKnockedBack)
                KnockbackVelocity = Vector3.zero;
        }
    }
}