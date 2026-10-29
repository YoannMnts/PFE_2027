using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires.Inventories;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Interactions.Chests
{
    public class RuntimeChest : MonoPhaseListener<InventoryPhase>, IInteractable
    {
        private InventoryPhase currentInventoryPhase;
        
        public void LaunchInteract()
        {
            var inventoryPhase = new InventoryPhase();
            inventoryPhase.Run();
            Debug.Log($"[Runtime Chest] interact]");
        }

        protected override void OnPhaseBegin(InventoryPhase phase)
        {
            base.OnPhaseBegin(phase);
            currentInventoryPhase = phase;
            Debug.Log($"[Runtime Chest] inventory is launched]");
        }

        protected override void OnPhaseEnd(InventoryPhase phase)
        {
            base.OnPhaseEnd(phase);
            currentInventoryPhase = null;
            Debug.Log($"[Runtime Chest] inventory is launched]");
        }
    }
}