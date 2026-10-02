using PFE.Core.Scripts.Enemy.Attacks;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    public interface IRuntimeNpc<TInstance> : IDamageable
        where TInstance : class, INpcInstance
    {
        public TInstance Instance { get; }
        public NavMeshAgent NavMeshAgent { get; }
        public void Setup(TInstance instance);
        public void MoveTo(Vector3 destination);
    }
}
