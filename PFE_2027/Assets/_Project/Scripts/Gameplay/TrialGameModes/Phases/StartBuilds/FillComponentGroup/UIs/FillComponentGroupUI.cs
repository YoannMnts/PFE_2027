using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using PFE.Utilities.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public class FillComponentGroupUI : MonoPhaseListener<FillComponentGroupPhase>
    {
        [SerializeField] 
        private CanvasGroup group;

        [SerializeField]
        private Button validateButton;
        
        [SerializeField]
        private Transform componentGroupContainer;
        
        [SerializeField]
        private SelectedComponentListUI selectedComponentList;

        private FillComponentGroupPhase current;

        private void Awake()
        {
            group.Hide(0.3f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            validateButton.onClick.AddListener(OnValidateButtonClick);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            validateButton.onClick.RemoveAllListeners();
        }

        protected override void OnPhaseBegin(FillComponentGroupPhase phase)
        {
            base.OnPhaseBegin(phase);
            
            if(current != null)
                current.Cancel();
            
            current = phase;

            foreach (var player in phase.gameModeContext.trialGameMode.Players)
            {
                player.ComponentGroup.ComponentGroupData.UIPrefab.InstantiatePrefab(componentGroupContainer);
            }
            
            selectedComponentList.Connect(phase.selectedComponentDatas);

            group.Show(0.3f);
        }

        protected override void OnPhaseEnd(FillComponentGroupPhase phase)
        {
            base.OnPhaseEnd(phase);
            
            group.Hide(0.3f);
            selectedComponentList.Disconnect();
            current = null;
        }
        
        private void OnValidateButtonClick()
        {
            current.SetResult(true);
        }
    } 
}