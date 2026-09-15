using System;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Utilities.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PFE.Gameplay.Scripts.Phases
{
    public class TakeComponentGroupPhaseUI : MonoPhaseListener<TakeComponentGroupPhase>
    {
        [SerializeField] 
        private CanvasGroup canvasGroup;

        [SerializeField] 
        private Button validateButton;
        
        [SerializeField]
        private ComponentGroupListUI componentGroupListUI;

        private TakeComponentGroupPhase current;
        private ComponentGroupData currentComponentGroupData;

        private void Awake()
        {
            canvasGroup.Hide(0.3f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            validateButton.onClick.AddListener(OnComponentGroupValidate);
        }

        protected override void OnDisable()
        {
            validateButton.onClick.RemoveListener(OnComponentGroupValidate);
            
            base.OnDisable();
        }

        protected override void OnPhaseBegin(TakeComponentGroupPhase phase)
        {
            base.OnPhaseBegin(phase);
            
            if(current != null)
                current.Cancel();
            
            current = phase;
            
            componentGroupListUI.Connect(phase.groupDatas);
            
            canvasGroup.Show(0.3f);
        }

        protected override void OnPhaseEnd(TakeComponentGroupPhase phase)
        {
            base.OnPhaseEnd(phase);
            
            canvasGroup.Hide(0.3f);
            
            componentGroupListUI.Disconnect();
            
            current = null;
        }

        public void SetComponentGroup(ComponentGroupData groupData)
        {
            currentComponentGroupData = groupData;
        }

        private void OnComponentGroupValidate()
        {
            current.SetResult(currentComponentGroupData);
        }
    }
}
