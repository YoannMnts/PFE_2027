using System;
using System.Linq;
using Helteix.Tools;
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
        private Transform container;

        private TakeComponentGroupPhase current;
        private ComponentGroupData SelectedComponentGroup => currentComponentGroupDatas[currentIndex];
        
        private ComponentGroupData[] currentComponentGroupDatas;
        private int currentIndex = 0;
        
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

            currentComponentGroupDatas = phase.groupDatas.ToArray();
            currentComponentGroupDatas[currentIndex]?.UIPrefab.InstantiatePrefab(container);
            
            canvasGroup.Show(0.3f);
        }

        protected override void OnPhaseEnd(TakeComponentGroupPhase phase)
        {
            base.OnPhaseEnd(phase);
            
            canvasGroup.Hide(0.3f);

            currentIndex = 0;
            currentComponentGroupDatas = null;
            current = null;
        }

        private void OnComponentGroupValidate()
        {
            current.SetResult(SelectedComponentGroup);
        }
    }
}
