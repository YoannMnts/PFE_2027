using System;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyInstance>, IPhaseListener<ProtectPilgrimPhase>
    {
        [SerializeField]
        private NavMeshAgent navMeshAgent;

        private PilgrimInstance pilgrimInstance;

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        private void LateUpdate()
        {
            if(pilgrimInstance == null)
                return;
            
            MoveTo(pilgrimInstance.CurrentPosition);
        }

        public void MoveTo(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out var hit, 2f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }

        [Button, DisableInEditorMode]
        public void DebugMoveTo(Vector3 position)
        {
            MoveTo(position);
        }

        public void OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrimInstance = phase.pilgrimInstance;
        }

        public void OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            pilgrimInstance = null;
        }
    }
}
