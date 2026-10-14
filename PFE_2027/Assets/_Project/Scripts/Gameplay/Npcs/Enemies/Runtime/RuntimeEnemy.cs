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
            phase.pilgrimInstance.OnPositionChanged += OnPilgrimMoved;
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            phase.pilgrimInstance.OnPositionChanged -= OnPilgrimMoved;
        }

        // The runtime only forwards what happens in the world; the instance and its behaviour pick the target
        protected virtual void OnPilgrimMoved(Vector3 pilgrimPosition) => Instance?.SetGoal(pilgrimPosition);
    }
}
