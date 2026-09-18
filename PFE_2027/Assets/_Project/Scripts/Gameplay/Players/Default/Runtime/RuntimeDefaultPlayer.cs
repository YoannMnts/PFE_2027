using Helteix.Tools;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.Players.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

        [Button, HideInEditorMode]
        public void DebugSpawnAttack(ComponentData componentData) => SpawnAttack(componentData);
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
            Player.ShowUI.OnValueChanged += SetUIMode;
            Player.OnComponentTrigger += SpawnAttack;
        }

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;
            Player.OnComponentTrigger -= SpawnAttack;
        }

        private void SetUIMode(bool showUI)
        {
            playerInput.SwitchCurrentActionMap(showUI ? "UI" : "Player");
            Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showUI;
        }

        private void SpawnAttack(ComponentData data)
        {
            Vector3 spawnPos = mesh.position + mesh.forward * spawnForwardDistance + Vector3.up * spawnUpDistance;
            Quaternion spawnRot = Quaternion.LookRotation(mesh.forward);
            var instance = data.AttackPrefab.InstantiatePrefab();
            instance.rotation = spawnRot;
            instance.position = spawnPos;
            instance.SetParent(attackContainer);

            if (instance.TryGetComponent<RuntimeComponent>(out var runtime))
            {
                runtime.Setup();
            }
        }
    }
}
