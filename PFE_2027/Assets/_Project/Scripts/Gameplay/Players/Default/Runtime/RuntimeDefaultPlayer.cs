using PFE.Gameplay.Scripts.Players.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : RuntimePlayer<DefaultPlayer>
    {
        [SerializeField]
        private Rigidbody rigidBody;
        
        [SerializeField]
        private PlayerInput playerInput;
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
            Player.ShowUI.OnValueChanged += SetUIMode;
        }

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;
        }

        private void SetUIMode(bool showUI)
        {
            playerInput.SwitchCurrentActionMap(showUI ? "UI" : "Player");
            Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showUI;
        }
    }
}
