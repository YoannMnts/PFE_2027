using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.BulletTimes;
using PFE.Gameplay.Scripts.Players.Runtime;
using PFE.Utilities.Scripts.Timers;
using Sirenix.OdinInspector;
using TraversalPro;
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

        // Same View transform as the CharacterRun: stick inputs are relative to it
        [SerializeField, BoxGroup("References")]
        private Transform view;
        
        private BulletTimePhase currentBulletTimePhase;

        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            Player.ShowUI.OnValueChanged += SetUIMode;
            
            if (characterMotor != null)
                characterMotor.Moving += OnCharacterMoving;
            else
                Debug.LogError("[RuntimeDefaultPlayer] No CharacterMotor assigned, movement won't be locked during attacks.", this);

            InitializeAnimation();
            
            var context = new AnimationContext(actionAnimator, hitboxAnchors, this, hitMask, OnDefaultHit, OnWindowEnter);
            // The player is not IDamageable yet: no owner to exclude
            animationRunner = new AnimationRunner(context);
            
            InitializeFacing();
        }

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving -= OnCharacterMoving;
            
            DisposeFacing();
        }

        private void LateUpdate()
        {
            UpdateLocomotionAnimation();
            UpdateFocus();
            
            if (animationRunner == null)
                return;

            animationRunner.LateTick();
            TryConsumeAttackRequest();
        }

        // Called by the CharacterMotor every FixedUpdate, after CharacterRun wrote the velocity goal
        // and before the motor applies it: every movement constraint goes here.
        private void OnCharacterMoving(ICharacterMotor motor)
        {
            // Read the player's intent before the constraints modify it
            CaptureMoveIntent(motor);
            LockMovementDuringAttack(motor);
            // After the lock: a displacement window (dash, lunge) moves the player even when its input is locked
            ApplyDisplacement(motor);
            ApplyPilgrimLeash(motor);
            
            ComputeTimeScale(motor);
        }

        private void LockMovementDuringAttack(ICharacterMotor motor)
        {
            if (animationRunner == null || !animationRunner.Has<MovementLock>(out _))
                return;
            
            motor.MoveInput = Vector3.zero;
            motor.LocalVelocityGoal = Vector3.zero;
        }

        private void ComputeTimeScale(ICharacterMotor motor)
        {
            motor.MoveInput *= TimeScaleManager.TimeMultiplier;
            motor.LocalVelocityGoal *= TimeScaleManager.TimeMultiplier;
        }

        private void SetUIMode(bool showUI)
        {
            playerInput.SwitchCurrentActionMap(showUI ? "UI" : "Player");
            Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showUI;
        }
    }
}
