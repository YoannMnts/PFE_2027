using System;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyInstance>, IPhaseListener<ProtectPilgrimPhase>
    {
        private Vector3 pilgrimPosition;

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
            pilgrimPosition = phase.pilgrimInstance.CurrentPosition;
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            pilgrimPosition = Vector3.zero;
        }
    }
}
