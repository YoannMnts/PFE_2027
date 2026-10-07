using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : IPhaseListener<BattlePhase>, IPhaseListener<GenerateAreaPhase>,
        IPhaseListener<ProtectPilgrimPhase>
    {
        private void OnEnable()
        {
            this.Register<BattlePhase>();
            this.Register<GenerateAreaPhase>();
            this.Register<ProtectPilgrimPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<BattlePhase>();
            this.Unregister<GenerateAreaPhase>();
            this.Unregister<ProtectPilgrimPhase>();
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

        public void OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrim = phase.pilgrimInstance;
            var runtimePilgrim = GetComponentInChildren<RuntimePilgrim>();
            runtimePilgrim.AddMemberToCmGroup(rigidBody.transform, targetGroupWeight, 1);
        }

        public void OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            pilgrim = null;
            var runtimePilgrim = GetComponentInChildren<RuntimePilgrim>();
            runtimePilgrim.RemoveMemberFromCmGroup(rigidBody.transform);
        }
    }
}