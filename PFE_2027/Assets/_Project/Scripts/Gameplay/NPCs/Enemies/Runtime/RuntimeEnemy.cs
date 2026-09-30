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
        

        [Button, DisableInEditorMode]
        public void DebugMoveTo(Vector3 position)
        {
            MoveTo(position);
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrimInstance = phase.pilgrimInstance;
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            pilgrimInstance = null;
        }
    }
}
