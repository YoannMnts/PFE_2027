using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Interactions
{
    public class Chest
    {
        private Condition interactableCondition;
        private ChannelKey interactStateKey;

        public void Start()
        {
            interactableCondition = new Condition(false);
            interactStateKey = ChannelKey.GetUniqueChannelKey();
            interactableCondition.AddCondition(interactStateKey, false);
        }
        
        public bool TryLaunchInteractable(bool isInteractable)
        {
            interactableCondition.Write(interactStateKey, isInteractable);

            if (!interactableCondition.Value)
            {
                return false;
            }
            
            Interact();
             
            Debug.Log($"{this} try interact");
            return true;
        }

        public void Interact()
        {
            //TODO comportement de l'interact 
            //pk pas activer la phase d'inventaire ? 
        }
    }
}