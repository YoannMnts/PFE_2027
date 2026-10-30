using System;
using Helteix.Tools.UI;
using PFE.Core.Scripts.Dialogues;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class ChoiceUIItem :  UIItem<ChoiceData>
    {
        [SerializeField] private Button choiceButton;
        [SerializeField] private TMP_Text choiceText;
        
        private ChoiceData currentChoice;
        private ChoicePhaseUI choicePhaseUI;

        private void Awake()
        {
            choicePhaseUI = GetComponentInParent<ChoicePhaseUI>();
        }

        protected override void SyncUI(ChoiceData current)
        {
            if(currentChoice != null)
                return;
            
            currentChoice = current;
            choiceButton.onClick.AddListener(ChooseTalk);
            choiceText.text = current.Text;
        }

        private void ChooseTalk()
        {
            choicePhaseUI.ChooseTalk();
        }

        protected override void ClearUI()
        {
        }
    }
}