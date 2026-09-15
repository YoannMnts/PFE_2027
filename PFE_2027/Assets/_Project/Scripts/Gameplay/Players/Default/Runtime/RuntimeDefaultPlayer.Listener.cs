using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : IPhaseListener<FightPhase>, IPhaseListener<GenerateArenaPhase>
    {
        private void OnEnable()
        {
            this.Register<FightPhase>();
            this.Register<GenerateArenaPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<FightPhase>();
            this.Unregister<GenerateArenaPhase>();
        }

        public void OnPhaseBegin(FightPhase phase)
        {
        }

        public void OnPhaseEnd(FightPhase phase)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
        }

        public void OnPhaseBegin(GenerateArenaPhase phase)
        {
        }

        public void OnPhaseEnd(GenerateArenaPhase phase)
        {
            transform.position = Vector3.zero;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
}