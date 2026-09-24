using Helteix.Tools;
using PFE.Core.Scripts;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.Enemy.Runtime;
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

        [SerializeField, BoxGroup("Attack")]
        private Vector3 hitBoxHalfExtents = new(.5f, .5f, .5f);

        [SerializeField, BoxGroup("Attack")]
        private LayerMask hitMask;

        private readonly Collider[] hitResults = new Collider[16];

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

        public void CastBasicAttack()
        {
            int count = Physics.OverlapBoxNonAlloc(GetHitBoxCenter(), hitBoxHalfExtents, hitResults,
                mesh.rotation, hitMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                RuntimeEnemy enemy = hitResults[i].GetComponentInParent<RuntimeEnemy>();
                if (enemy != null)
                    enemy.Damage(GameMetricsSettings.Current.BasicAttackDamage);
            }
        }

        private Vector3 GetHitBoxCenter()
            => mesh.position + mesh.forward * spawnForwardDistance + Vector3.up * spawnUpDistance;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (mesh == null)
                return;

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            Gizmos.matrix = Matrix4x4.TRS(GetHitBoxCenter(), mesh.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, hitBoxHalfExtents * 2f);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }
}
