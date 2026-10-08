using Helteix.Tools;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Enemy.Attacks.BasicAttacks;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Players.Runtime;
using Sirenix.OdinInspector;
using TraversalPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : RuntimePlayer<DefaultPlayer>
    {
        [SerializeField, BoxGroup("References", true, false, 1f)]
        private Rigidbody rigidBody;

        [SerializeField, BoxGroup("References")] 
        private Transform mesh;
        
        [SerializeField, BoxGroup("References")]
        private PlayerInput playerInput;
        
        [SerializeField, BoxGroup("References")]
        private Animator animator;

        [SerializeField, BoxGroup("References")]
        private CharacterMotor characterMotor;
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            Player.ShowUI.OnValueChanged += SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving += OnCharacterMoving;
            else
                Debug.LogError("[RuntimeDefaultPlayer] No CharacterMotor assigned, movement won't be locked during attacks.", this);

            var attackContext = new AttackContext(animator, hitboxAnchors, this, hitMask, OnAttackHit);
            // The player is not IDamageable yet: no owner to exclude
            attackRunner = new AttackRunner(attackContext);

            InitializeFacing();
        }

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving -= OnCharacterMoving;
        }

        // Called by the CharacterMotor every FixedUpdate, after CharacterRun wrote the velocity goal
        // and before the motor applies it: every movement constraint goes here.
        private void OnCharacterMoving(ICharacterMotor motor)
        {
            // Read the player's intent before the constraints modify it
            CaptureMoveIntent(motor);
            LockMovementDuringAttack(motor);
            ApplyPilgrimLeash(motor);
        }

        private void LockMovementDuringAttack(ICharacterMotor motor)
        {
            if (attackRunner == null || !attackRunner.Has(AttackFlags.MovementLock))
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
    }
}
