using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires.Inventories;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : 
        IPhaseListener<BattlePhase>, 
        IPhaseListener<ProtectPilgrimPhase>,
        IPhaseListener<CampfirePhase>, 
        IPhaseListener<DialoguePhase>, 
        IPhaseListener<GenerateNpcPhase>, 
        IPhaseListener<CleanupPhase>, 
        IPhaseListener<InventoryPhase>
    {
        private void OnEnable()
        {
            this.Register<BattlePhase>();
            this.Register<CampfirePhase>();
            this.Register<ProtectPilgrimPhase>();
            this.Register<DialoguePhase>();
            this.Register<GenerateNpcPhase>();
            this.Register<CleanupPhase>();
            this.Register<InventoryPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<BattlePhase>();
            this.Unregister<CampfirePhase>();
            this.Unregister<ProtectPilgrimPhase>();
            this.Unregister<DialoguePhase>();
            this.Unregister<GenerateNpcPhase>();
            this.Unregister<CleanupPhase>();
            this.Unregister<InventoryPhase>();
        }

        void IPhaseListener<BattlePhase>.OnPhaseBegin(BattlePhase phase)
        {
        }

        void IPhaseListener<BattlePhase>.OnPhaseEnd(BattlePhase phase)
        {
        }


        void IPhaseListener<CampfirePhase>.OnPhaseBegin(CampfirePhase phase)
        {
        }

        void IPhaseListener<CampfirePhase>.OnPhaseEnd(CampfirePhase phase)
        {
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

        void IPhaseListener<GenerateNpcPhase>.OnPhaseBegin(GenerateNpcPhase phase)
        {
            var playerSpawn = phase.area.SpawnPosition;
            
            rigidBody.position = playerSpawn == null ? Vector3.zero : playerSpawn.position;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }

        void IPhaseListener<GenerateNpcPhase>.OnPhaseEnd(GenerateNpcPhase phase)
        {
        }

        void IPhaseListener<CleanupPhase>.OnPhaseBegin(CleanupPhase phase)
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        void IPhaseListener<CleanupPhase>.OnPhaseEnd(CleanupPhase phase)
        {
        }

        void IPhaseListener<InventoryPhase>.OnPhaseBegin(InventoryPhase phase)
        {
            
        }

        void IPhaseListener<InventoryPhase>.OnPhaseEnd(InventoryPhase phase)
        {
        }
    }
}