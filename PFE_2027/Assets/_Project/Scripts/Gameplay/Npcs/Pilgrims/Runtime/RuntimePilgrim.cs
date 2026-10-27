using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.Interactions;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players.Default.Runtime;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : RuntimeNpc<PilgrimInstance, PilgrimData>, IInteractable,
        IPhaseListener<ProtectPilgrimPhase>, 
        IPhaseListener<CampPhase>
    {
        [SerializeField, BoxGroup("Debugs")]
        private bool isCameraOnPilgrim = true;
        
        [SerializeField, BoxGroup("Debugs")]
        private CinemachineCamera targetCamera;
        
        [SerializeField, BoxGroup("References")]
        private CinemachineTargetGroup targetGroup;

        private Action interaction;
        private CrossRoadGameModeContext gameModeContext;
        private CampPhase campPhase;



        //--------------------------------------------------
        

        protected override void OnSetup(CrossRoadGameModeContext context)
        {
            base.OnSetup(context);
            this.gameModeContext = context;
            
            if (!isCameraOnPilgrim)
                targetCamera.Priority.Value = -1;
        }

        private void OnEnable()
        {
            this.Register<ProtectPilgrimPhase>();
            this.Register<CampPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<ProtectPilgrimPhase>();
            this.Unregister<CampPhase>();
        }
        
        //--------------------------------------------------

        public void AddMemberToCmGroup(Transform member, float weight = 0, float radius = 0)
        {
            targetGroup.AddMember(member, weight, radius);
        }
        public void RemoveMemberFromCmGroup(Transform member)
        {
            targetGroup.RemoveMember(member);
        }
        
        //--------------------------------------------------

        
        private void FixedUpdate()
        {
            if (Instance != null)
                Instance.UpdatePosition(transform.position);
        }
        
        
        public void LaunchInteract()
        {
            interaction?.Invoke();
        }
        private void BattleInteraction()
        {
            Debug.Log($"{this} is interacting on battle");
        }
        
        private void CampInteraction()
        {
            Debug.Log($"{this} is interacting on camp");
            var dialoguePhase = new DialoguePhase(gameModeContext);
            dialoguePhase.Run();
        }
        
        
        //--------------------------------------------------


        public void OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            interaction = BattleInteraction;
        }
        
        public void OnPhaseEnd(ProtectPilgrimPhase phase)
        {
        }

        public void OnPhaseBegin(CampPhase phase)
        {
            interaction = CampInteraction;
            campPhase = phase;
        }

        public void OnPhaseEnd(CampPhase phase)
        {
        }
    }
}
