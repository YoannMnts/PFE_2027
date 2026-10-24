using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Interactions.Chests
{
    public class RuntimeChest : MonoBehaviour, IInteractable
    {
        public bool IsInteractable(bool interactionPossibility)
        {
            //TODO ici trouver un moyen de bien séparer le runtime de la logic
            /*
            if (interactionPossibility)
            {
                instance.TryLaunchInteractable(interactionPossibility);
                return true;
            }
            */
            return false;
        }
    }
}