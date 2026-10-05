using Helteix.Tools;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Enemy.Attacks.BasicAttacks;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.Players.Runtime;
using Sirenix.OdinInspector;
using TraversalPro;
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
        private BasicAttackData basicAttack;
        
        [SerializeField, BoxGroup("Attack")] 
        private HitboxAnchors hitboxAnchors;

        [SerializeField, BoxGroup("Attack")]
        private LayerMask hitMask;
        
        [SerializeField, BoxGroup("Animation")]
        private Animator animator;

        [SerializeField, BoxGroup("Movement")]
        private CharacterMotor characterMotor;
        
        private AttackRunner attackRunner;
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            Player.ShowUI.OnValueChanged += SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving += LockMovementDuringAttack;
            else
                Debug.LogError("[RuntimeDefaultPlayer] No CharacterMotor assigned, movement won't be locked during attacks.", this);

            attackRunner = new AttackRunner(animator, hitboxAnchors, null, hitMask);
        }

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving -= LockMovementDuringAttack;
        }
        
        private void LateUpdate()
        {
            attackRunner?.LateTick();
        }

        private void LockMovementDuringAttack(ICharacterMotor motor)
        {
            if (attackRunner == null || !attackRunner.IsMovementLocked)
                return;

            motor.MoveInput = Vector3.zero;
            motor.LocalVelocityGoal = Vector3.zero;
        }

        private void SetUIMode(bool showUI)
        {
            playerInput.SwitchCurrentActionMap(showUI ? "UI" : "Player");
            Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showUI;
        }

        public void PlayAttack()
        {
            if (attackRunner.IsRunning)
                return;

            attackRunner.Begin(basicAttack);
        }
    }
}
