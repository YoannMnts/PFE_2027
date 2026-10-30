using System;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Utilities.Scripts;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class ChoicePhaseUI : MonoPhaseListener<ChoicePhase>
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private ChoiceUIList choiceList;
        
        private ChoicePhase currentPhase;

        private void Awake()
        {
            group.Hide();
        }

        protected override void OnPhaseBegin(ChoicePhase phase)
        {
            base.OnPhaseBegin(phase);

            if(currentPhase != null)
                currentPhase.Cancel();
            
            currentPhase = phase;
            
            var choices = phase.ChoiceDatas;
            
            choiceList.Connect(choices);
            group.Show();
        }

        protected override void OnPhaseEnd(ChoicePhase phase)
        {
            base.OnPhaseEnd(phase);

            group.Hide();
            choiceList.Disconnect();
            currentPhase = null;
        }

        public void ChooseTalk()
        {
            currentPhase.SetResult(true);
        }
    }
}