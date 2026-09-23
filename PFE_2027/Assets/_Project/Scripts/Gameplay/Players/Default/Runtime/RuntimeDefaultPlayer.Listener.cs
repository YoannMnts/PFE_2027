using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : IPhaseListener<BattlePhase>, IPhaseListener<GenerateAreaPhase>
    {
        private void OnEnable()
        {
            this.Register<BattlePhase>();
            this.Register<GenerateAreaPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<BattlePhase>();
            this.Unregister<GenerateAreaPhase>();
        }

        public void OnPhaseBegin(BattlePhase phase)
        {
        }

        public void OnPhaseEnd(BattlePhase phase)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
        }

        public void OnPhaseBegin(GenerateAreaPhase phase)
        {
        }

        public void OnPhaseEnd(GenerateAreaPhase phase)
        {
            transform.position = Vector3.zero;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}