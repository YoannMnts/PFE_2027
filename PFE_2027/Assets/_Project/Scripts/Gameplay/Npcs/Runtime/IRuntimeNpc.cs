using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    public interface IRuntimeNpc : IDamageable
    {
        public NavMeshAgent NavMeshAgent { get; }
        public bool IsKnockedBack { get; }
        public HitboxRunner HitboxRunner { get; }
        public void ApplyKnockback(in KnockbackRequest request);
    }
}
