using System;
using Helteix.Tools.UI;
using PFE.Core.Scripts.ComponentSystem;
using UnityEngine;
using UnityEngine.UI;

namespace PFE.Gameplay.Scripts.Phases
{
    [RequireComponent(typeof(Button))]
    public class ComponentGroupUI : UIItem<ComponentGroupData>
    {
        [SerializeField]
        private Button selectButton;
        
        private TakeComponentGroupPhaseUI takeComponentGroupPhaseUI;
        private ComponentGroupData componentGroupData;

        private void Awake()
        {
            takeComponentGroupPhaseUI = GetComponentInParent<TakeComponentGroupPhaseUI>();
        }

        private void OnEnable()
        {
            selectButton.onClick.AddListener(OnSelect);
        }

        private void OnDisable()
        {
            selectButton.onClick.RemoveListener(OnSelect);
        }

        protected override void SyncUI(ComponentGroupData current)
        {
            componentGroupData = current;
        }

        protected override void ClearUI()
        {
            componentGroupData = null;
        }

        private void OnSelect()
        {
            takeComponentGroupPhaseUI.SetComponentGroup(componentGroupData);
        }
    }
}