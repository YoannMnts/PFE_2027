using System;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Utilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class TalkPhaseUI : MonoPhaseListener<TalkPhase>
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text textField;
        [SerializeField] private Button skipButton;
        
        private TalkPhase currentPhase;

        private void Awake()
        {
            group.Hide();
        }


        protected override void OnPhaseBegin(TalkPhase phase)
        {
            base.OnPhaseBegin(phase);

            if (currentPhase != null)
            {
                currentPhase.Cancel();
                currentPhase = null;
            }
            
            currentPhase = phase;
            
            //declenche la methode SkipPhase quand clické
            skipButton.onClick.AddListener(SkipPhase);
            group.Show();

            ShowText();
        }


        protected override void OnPhaseEnd(TalkPhase phase)
        {
            base.OnPhaseEnd(phase);
            
            if (currentPhase != phase)
                return;
            
            group.Hide();
            skipButton.onClick.RemoveListener(SkipPhase);
            textField.text = string.Empty;
        }
        
        private void ShowText()
        {
            foreach (var text in currentPhase.texts)
            {
                textField.text += text;
            }
        }

        private void SkipPhase()
        {
            currentPhase?.SetResult(true);
        }
    }
}