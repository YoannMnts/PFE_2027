using Helteix.Tools;
using PFE.Gameplay.Scripts.Players.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : RuntimePlayer<DefaultPlayer>
    {
        [SerializeField]
        private Rigidbody rigidBody;

        [SerializeField] 
        private Transform mesh;
        
        [SerializeField]
        private PlayerInput playerInput;

        [SerializeField, BoxGroup("Attack")]
        private float spawnForwardDistance = 1f;
        
        [SerializeField, BoxGroup("Attack")]
        private float spawnUpDistance = .5f;

        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
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
