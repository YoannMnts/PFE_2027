using UnityEngine;
using UnityEngine.InputSystem;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public class CharacterControllerExtension : MonoBehaviour
    {
        private RuntimeDefaultPlayer runtimePlayer;

        private void Awake()
        {
            runtimePlayer = GetComponentInParent<RuntimeDefaultPlayer>();
        }

        public void PerformAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            runtimePlayer.PlayAttack();
        }
    }
}
