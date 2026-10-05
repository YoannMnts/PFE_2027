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

        [SerializeField, BoxGroup("References", true, false, 1f)]
        private Rigidbody rigidBody;

        [SerializeField, BoxGroup("References")] 
        private Transform mesh;
        
        [SerializeField, BoxGroup("References")]
        private PlayerInput playerInput;
        
        [SerializeField, BoxGroup("References")]
        private Animator animator;
        
        [SerializeField, BoxGroup("References")]
        private CinemachineTargetGroup targetGroup;

        [SerializeField, BoxGroup("References")]
        private CharacterMotor characterMotor;

        [SerializeField, BoxGroup("Pilgrim Leash"), Min(0.5f)]
        private float leashRadius = 8f;

        // Speed at which the player is pulled back when the pilgrim got too far away
        [SerializeField, BoxGroup("Pilgrim Leash"), Min(0f)]
        private float leashPullBackSpeed = 4f;
        
        private AttackRunner attackRunner;
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            Player.ShowUI.OnValueChanged += SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving += OnCharacterMoving;
            else
                Debug.LogError("[RuntimeDefaultPlayer] No CharacterMotor assigned, movement won't be locked during attacks.", this);

            attackRunner = new AttackRunner(animator, hitboxAnchors, null, hitMask);
        }
        

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving -= OnCharacterMoving;
        }
        
        private void LateUpdate()
        {
            attackRunner?.LateTick();
        }

        // Called by the CharacterMotor every FixedUpdate, after CharacterRun wrote the velocity goal
        // and before the motor applies it: every movement constraint goes here.
        private void OnCharacterMoving(ICharacterMotor motor)
        {
            LockMovementDuringAttack(motor);
            ApplyPilgrimLeash(motor);
        }

        public void AddMemberToCmGroup(Transform member, float weight = 0, float radius = 0)
        {
            targetGroup.AddMember(member, weight, radius);
        }

        public void RemoveMemberFromCmGroup(Transform member)
        {
            targetGroup.RemoveMember(member);
        }

        private void LockMovementDuringAttack(ICharacterMotor motor)
        {
            if (attackRunner == null || !attackRunner.IsMovementLocked)
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
            if (attackRunner.IsRunning)
                return;

            attackRunner.Begin(basicAttack);
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
