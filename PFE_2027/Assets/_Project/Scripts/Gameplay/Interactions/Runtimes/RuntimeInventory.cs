using UnityEngine;

namespace PFE.Gameplay.Scripts.Interactions.Chests
{
    public class RuntimeInventory : MonoBehaviour, IInteractable
    {
        public bool TryLaunchInteractable()
        {
            Debug.Log($"{name} try interact");
            return true;
        }
    }
}