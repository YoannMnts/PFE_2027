using PFE.Utilities.Scripts.Timers;
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

        public void PerformDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                runtimePlayer.StartCharging();
            }

            if (context.canceled)
            {
                runtimePlayer.Dash();
            }
        }

        public void PerformInteract(InputAction.CallbackContext context)
        {
            //Debug.Log("Performing interact");
            if (!context.performed)
                return;
            
            
            runtimePlayer.Interact();
        }

        public void PerformFocus(InputAction.CallbackContext context)
        {
            if(!context.performed)
                return;
            
            runtimePlayer.Focus();
        }
    }
}
