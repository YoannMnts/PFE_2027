using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using UnityEngine.PlayerLoop;

namespace PFE.Gameplay.Scripts.Interactions
{
    public interface IInteractable
    {
        public bool TryLaunchInteractable(bool isInteractable);
        public void Interact();
    }
}