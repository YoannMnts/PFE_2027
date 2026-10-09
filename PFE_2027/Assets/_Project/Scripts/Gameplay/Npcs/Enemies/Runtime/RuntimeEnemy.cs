using System;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyInstance, EnemyData>, IPhaseListener<ProtectPilgrimPhase>
    {
        protected override Vector3 TargetPosition => targetPosition;

        private Vector3 targetPosition;
        
        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            phase.pilgrimInstance.OnPositionChanged += UpdateTargetPosition;
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            phase.pilgrimInstance.OnPositionChanged -=  UpdateTargetPosition;
        }


        private void UpdateTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }
    }
}
