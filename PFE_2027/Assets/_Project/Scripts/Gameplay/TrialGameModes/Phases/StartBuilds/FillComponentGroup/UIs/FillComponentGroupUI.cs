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
        }

        protected override void OnPhaseEnd(FillComponentGroupPhase phase)
        {
            base.OnPhaseEnd(phase);
            
            current = null;
        }
        
        private void OnValidateButtonClick()
        {
            current.SetResult(true);
        }
    } 
}