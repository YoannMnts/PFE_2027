using System;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;
using UnityEngine.UI;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [RequireComponent(typeof(Button))]
    public class ValidateGroupButton : MonoPhaseListener<FillGroupComponentPhase>
    {
        private Button button;
        private FillGroupComponentPhase current;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            button.onClick.AddListener(OnButtonClick);
        }

        protected override void OnDisable()
        {
            button.onClick.RemoveListener(OnButtonClick);
            
            base.OnDisable();
        }

        protected override void OnPhaseBegin(FillGroupComponentPhase phase)
        {
            base.OnPhaseBegin(phase);
            
            if(current != null)
                current.Cancel();
            
            current = phase;
        }

        protected override void OnPhaseEnd(FillGroupComponentPhase phase)
        {
            current = null;
            
            base.OnPhaseEnd(phase);
        }

        private void OnButtonClick()
        {
            current?.SetResult(true);
        }
    }
}