using PFE.Core.Scripts.Enemy.Attacks;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    public interface IRuntimeNpc : IDamageable
    {
        public NavMeshAgent NavMeshAgent { get; }
        public bool IsKnockedBack { get; }
        public void MoveTo(Vector3 destination);
        public void ApplyKnockback(Vector3 offset);
    }
    
    public interface IRuntimeNpc<TInstance> : IRuntimeNpc
        where TInstance : class, INpcInstance
    {
        public TInstance Instance { get; }
        public void Setup(TInstance instance);
    }
}
