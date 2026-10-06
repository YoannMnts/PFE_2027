using Helteix.Tools;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Enemy.Attacks.BasicAttacks;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.Enemy.Runtime;
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
        [SerializeField, BoxGroup("Attack", true, false, -1f)] 
        private BasicAttackData basicAttack;
        
        [SerializeField, BoxGroup("Attack")] 
        private HitboxAnchors hitboxAnchors;

        [SerializeField, BoxGroup("Attack")]
        private LayerMask hitMask;

        // How long (in seconds) an attack press stays valid while waiting for the combo window to open
        [SerializeField, BoxGroup("Attack"), Min(0f)]
        private float attackInputBuffer = 0.25f;

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

        [SerializeField, BoxGroup("Pilgrim Leash"), Min(0.5f)]
        private float leashRadius = 8f;

        // Speed at which the player is pulled back when the pilgrim got too far away
        [SerializeField, BoxGroup("Pilgrim Leash"), Min(0f)]
        private float leashPullBackSpeed = 4f;
        
        [SerializeField, BoxGroup("Pilgrim Leash"), Min(0f)]
        private float targetGroupWeight = .5f;
        
        private AttackRunner attackRunner;
        private float attackRequestTime = float.NegativeInfinity;
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            Player.ShowUI.OnValueChanged += SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving += OnCharacterMoving;
            else
                Debug.LogError("[RuntimeDefaultPlayer] No CharacterMotor assigned, movement won't be locked during attacks.", this);

            attackRunner = new AttackRunner(animator, hitboxAnchors, mesh.transform, hitMask);
        }
        

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving -= OnCharacterMoving;
        }
        
        private void LateUpdate()
        {
            if (attackRunner == null)
                return;

            attackRunner.LateTick();
            TryConsumeAttackRequest();
        }

        // Called by the CharacterMotor every FixedUpdate, after CharacterRun wrote the velocity goal
        // and before the motor applies it: every movement constraint goes here.
        private void OnCharacterMoving(ICharacterMotor motor)
        {
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

        // Keeps the player inside a horizontal circle around the pilgrim. At the edge, the outward part
        // of the velocity goal is removed so the player slides along the circle instead of leaving it.
        private void ApplyPilgrimLeash(ICharacterMotor motor)
        {
            if (leashedPilgrim == null || leashedPilgrim.IsDead)
                return;

            Vector3 fromCenter = motor.Rigidbody.position - leashedPilgrim.CurrentPosition;
            fromCenter.y = 0f;

            float distance = fromCenter.magnitude;
            if (distance < leashRadius)
                return;

            Vector3 outward = fromCenter / distance;
            Vector3 goal = motor.LocalVelocityGoal;

            float outwardSpeed = Vector3.Dot(goal, outward);
            if (outwardSpeed > 0f)
                goal -= outward * outwardSpeed;

            // Already outside (the pilgrim walked away): pull the player back towards the circle
            if (distance > leashRadius + 0.1f)
                goal -= outward * leashPullBackSpeed;

            motor.LocalVelocityGoal = goal;
        }

        private void SetUIMode(bool showUI)
        {
            playerInput.SwitchCurrentActionMap(showUI ? "UI" : "Player");
            Cursor.lockState = showUI ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = showUI;
        }

        public void PlayAttack()
        {
            attackRequestTime = Time.time;
            TryConsumeAttackRequest();
        }

        private void TryConsumeAttackRequest()
        {
            if (Time.time - attackRequestTime > attackInputBuffer)
                return;

            if (!attackRunner.IsRunning)
                attackRunner.Begin(basicAttack);
            else if (attackRunner.Has(AttackFlags.Combo) && attackRunner.Current.Next != null)
                attackRunner.Begin(attackRunner.Current.Next);
            else
                return; // Keep the request buffered until the combo window opens or it expires

            attackRequestTime = float.NegativeInfinity;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (leashedPilgrim == null)
                return;

            UnityEditor.Handles.color = new Color(1f, 0f, 0.07f, 1f);
            UnityEditor.Handles.DrawWireDisc(leashedPilgrim.CurrentPosition, Vector3.up * 2, leashRadius);
        }
#endif
    }
}
