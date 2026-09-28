using Helteix.Tools;
using PFE.Core.Scripts;
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
        private float spawnForwardDistance = 1f;
        
        [SerializeField, BoxGroup("Attack")]
        private float spawnUpDistance = .5f;

        [SerializeField, BoxGroup("Attack")]
        private Vector3 hitBoxHalfExtents = new(.5f, .5f, .5f);

        [SerializeField, BoxGroup("Attack")]
        private LayerMask hitMask;
        
        [SerializeField, BoxGroup("Animation")]
        private Animator animator;

        [SerializeField, BoxGroup("Movement")]
        private CharacterMotor characterMotor;

        private static readonly int AttackTrigger = Animator.StringToHash("OnAttack");
        // nom de l'état d'attaque dans le layer 0 du PlayerAnimator
        private static readonly int AttackStateHash = Animator.StringToHash("StaffAttack");
        private readonly Collider[] hitResults = new Collider[16];

        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            Player.ShowUI.OnValueChanged += SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving += LockMovementDuringAttack;
            else
                Debug.LogError("[RuntimeDefaultPlayer] No CharacterMotor assigned, movement won't be locked during attacks.", this);
        }

        protected override void OnDisconnected()
        {
            Player.ShowUI.OnValueChanged -= SetUIMode;

            if (characterMotor != null)
                characterMotor.Moving -= LockMovementDuringAttack;
        }

        // Appelé par le CharacterMotor à chaque FixedUpdate, après que CharacterRun a écrit la vitesse voulue
        // et avant que le motor ne l'applique : on l'écrase pendant l'attaque. CharacterRun reste actif
        // pour continuer à recevoir l'input (sinon un relâchement de touche pendant l'attaque serait perdu).
        private void LockMovementDuringAttack(ICharacterMotor motor)
        {
            if (!IsAttacking())
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
            // évite que le trigger reste armé pendant l'anim et rejoue une 2e attaque (buffer prévu avec la tool hitbox)
            if (IsAttacking())
                return;

            animator.SetTrigger(AttackTrigger);
            CastBasicAttack();
        }

        // vrai pendant l'état Attack (fondu de sortie compris) et pendant la transition qui y entre
        private bool IsAttacking()
        {
            if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AttackStateHash)
                return true;

            return animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).shortNameHash == AttackStateHash;
        }

        private void CastBasicAttack()
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
