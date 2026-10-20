using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : IPhaseListener<BattlePhase>, IPhaseListener<GenerateAreaPhase>,
        IPhaseListener<ProtectPilgrimPhase>, IPhaseListener<CampfirePhase>, IPhaseListener<DialoguePhase>
    {
        private void OnEnable()
        {
            this.Register<BattlePhase>();
            this.Register<CampfirePhase>();
            this.Register<GenerateAreaPhase>();
            this.Register<ProtectPilgrimPhase>();
            this.Register<DialoguePhase>();
        }

        private void OnDisable()
        {
            this.Unregister<BattlePhase>();
            this.Unregister<CampfirePhase>();
            this.Unregister<GenerateAreaPhase>();
            this.Unregister<ProtectPilgrimPhase>();
            this.Unregister<DialoguePhase>();
        }

        void IPhaseListener<BattlePhase>.OnPhaseBegin(BattlePhase phase)
        {
        }

        void IPhaseListener<BattlePhase>.OnPhaseEnd(BattlePhase phase)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
        }


        void IPhaseListener<CampfirePhase>.OnPhaseBegin(CampfirePhase phase)
        {
        }

        void IPhaseListener<CampfirePhase>.OnPhaseEnd(CampfirePhase phase)
        {
        }


        void IPhaseListener<GenerateAreaPhase>.OnPhaseBegin(GenerateAreaPhase phase)
        {
        }

        void IPhaseListener<GenerateAreaPhase>.OnPhaseEnd(GenerateAreaPhase phase)
        {
            transform.position = Vector3.zero;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        
        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrim = phase.pilgrimInstance;
            var runtimePilgrim = GetComponentInChildren<RuntimePilgrim>();
            runtimePilgrim.AddMemberToCmGroup(rigidBody.transform, targetGroupWeight, 1);
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            pilgrim = null;
            var runtimePilgrim = GetComponentInChildren<RuntimePilgrim>();
            runtimePilgrim.RemoveMemberFromCmGroup(rigidBody.transform);
        }

        void IPhaseListener<DialoguePhase>.OnPhaseBegin(DialoguePhase phase)
        {
            
        }

        void IPhaseListener<DialoguePhase>.OnPhaseEnd(DialoguePhase phase)
        {
            
        }
    }
}