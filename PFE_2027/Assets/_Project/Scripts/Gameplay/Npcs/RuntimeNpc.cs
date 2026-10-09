using System;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.NPCs;
using PrimeTween;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    [RequireComponent(typeof(BehaviorGraphAgent))]
    public abstract class RuntimeNpc : MonoBehaviour
    {
        protected const string BEHAVIOR_SELF_NPC = "SelfNpc";
        protected const string BEHAVIOR_TARGET_POSITION = "TargetPosition";
        protected const string BEHAVIOR_ATTACK_DATA = "AttackData";
        
        // Same parameters as the player controller (filled by Traversal Pro's CharacterAnimator on the player).
        // Hashed once: SetFloat with an int never builds or compares strings.
        private static readonly int VELOCITY_Y_ID = Animator.StringToHash("VelocityY");
        private static readonly int ANIMATION_SPEED_ID = Animator.StringToHash("AnimationSpeed");
        
        // Time for the animated speed to catch up with the real one (same default as Traversal Pro)
        private const float ANIMATION_SPEED_SMOOTH_TIME = 0.15f;
        
        [field : SerializeField]
        public NavMeshAgent NavMeshAgent { get; private set; }
        
        // NPC visual: it faces the movement direction, the root never rotates
        // (its children, like a camera or a UI above the head, keep their orientation).
        // Empty = previous behaviour: the agent rotates the whole root.
        [field : SerializeField]
        public Transform Mesh { get; private set; }
        
        // Animator of the model; its walk blend tree follows the agent's speed
        [field : SerializeField]
        public Animator Animator { get; private set; }
        
        // Converts the agent speed (m/s) into the blend tree value: up to 1 = idle → walk → run,
        // above 1 = run clip played faster. Default = Traversal Pro's curve for the UnityRobot clips.
        [SerializeField] 
        private AnimationCurve runSpeedToValue;
        
        protected abstract Vector3 TargetPosition { get;  }
        public AttackRunner AttackRunner { get; protected set;}
        
        private float animatedSpeed;
        private float animatedSpeedVelocity;
        protected BehaviorGraphAgent behaviorGraphAgent;
        
        
        // Unity messages are protected virtual: a child class declaring one must override and call base.X(),
        // otherwise it would silently hide this one.
        protected virtual void Awake()
        {
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            
            if (NavMeshAgent != null)
                NavMeshAgent.updateRotation = Mesh == null;
        }
        
        protected virtual void Update()
        {
            FaceMovement();
            UpdateAnimator();
        }
        
        protected virtual void LateUpdate()
        {
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_TARGET_POSITION, TargetPosition);
        }
        
        public virtual void MoveTo(Vector3 destination)
        {
            if (NavMesh.SamplePosition(destination, out var hit, 2f, NavMesh.AllAreas))
            {
                NavMeshAgent.SetDestination(hit.position);
            }
        }
        
        // Rotates the mesh towards the walking direction, at the turn speed set on the agent (Angular Speed)
        private void FaceMovement()
        {
            if (Mesh == null || NavMeshAgent == null)
                return;

            Vector3 velocity = NavMeshAgent.velocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            Mesh.rotation = Quaternion.RotateTowards(Mesh.rotation, targetRotation, NavMeshAgent.angularSpeed * Time.deltaTime);
        }
        
        // Feeds the walk blend tree with the agent's horizontal speed, converted by runSpeedToValue
        // (same pipeline as Traversal Pro's CharacterAnimator on the player)
        private void UpdateAnimator()
        {
            if (Animator == null || NavMeshAgent == null)
                return;

            Vector3 velocity = NavMeshAgent.velocity;
            velocity.y = 0f;

            animatedSpeed = Mathf.SmoothDamp(animatedSpeed, velocity.magnitude, ref animatedSpeedVelocity, ANIMATION_SPEED_SMOOTH_TIME);

            float runValue = runSpeedToValue.Evaluate(animatedSpeed);
            Animator.SetFloat(VELOCITY_Y_ID, runValue);
            Animator.SetFloat(ANIMATION_SPEED_ID, Mathf.Max(runValue, 1f));
        }
        
        protected virtual void OnModifyHealth(float currentHealth)
        {
            Tween.PunchScale(transform, Vector3.one * .2f, .3f);
        }
        
        protected virtual void OnDeath(INpcInstance deadInstance)
        {
            Tween.StopAll(onTarget: transform);
            Destroy(gameObject);
        }
    }
    
    
    // Base of NPC runtimes: receives its typed instance through Npc<TData, TInstance>.CreateInstance
    public abstract class RuntimeNpc<TInstance, TData> : RuntimeNpc, IRuntimeNpc<TInstance> 
        where TInstance : NpcInstance<TData>
        where TData : NpcData 
    {
        // Below this speed (m/s) the knockback is considered over
        private const float KNOCKBACK_STOP_SPEED = 0.05f;

        [SerializeField, BoxGroup("Attacks")]
        private HitboxAnchors hitboxAnchors;
        
        [SerializeField, BoxGroup("Attacks")]
        private LayerMask hitMask;
        
        private TInstance instance;
        public TInstance Instance => instance;
        
        protected float knockbackDamping;
        
        private Vector3 knockbackVelocity;
        
        public bool IsKnockedBack => knockbackVelocity.sqrMagnitude > KNOCKBACK_STOP_SPEED * KNOCKBACK_STOP_SPEED;
        
        public void Setup(TInstance npcInstance)
        {
            if (npcInstance == null)
            {
                Debug.LogError($"[RuntimeNpc] '{name}' received a null {typeof(TInstance).Name}.", this);
                return;
            }

            Unsubscribe();
            instance = npcInstance;
            instance.OnModifyHealth += OnModifyHealth;
            instance.OnDeath += OnDeath;

            knockbackDamping = instance.data.KnockbackDamping;

            behaviorGraphAgent.SetVariableValue(BEHAVIOR_SELF_NPC, this);
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_ATTACK_DATA, Instance.data.AttackData);

            var attackContext = new AttackContext(Animator, hitboxAnchors, this, hitMask, OnAttackHit);
            AttackRunner = new AttackRunner(attackContext);
            
            OnSetup();
        }

        protected override void Update()
        {
            UpdateKnockback();
            LockMovementDuringAttack();
            base.Update();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            
            Debug.Log($"[Runtime Npc] {name} runner {AttackRunner}");
            AttackRunner?.LateTick();
        }
        
        private void LockMovementDuringAttack()
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            bool isLocked = AttackRunner != null && AttackRunner.Has(AttackFlags.MovementLock);
            NavMeshAgent.isStopped = isLocked;

            if (isLocked)
                NavMeshAgent.velocity = Vector3.zero;
        }

        protected virtual void OnDestroy()
        {
            Unsubscribe();
        }

        [Button, DisableInEditorMode]
        public void DebugDamage(int damage)
        {
            TakeDamage(damage);
        }
        
        public void TakeDamage(int amount)
        {
            instance?.AddOrRemoveHealth(-amount);
        }
        
        public void ApplyKnockback(Vector3 offset)
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            offset.y = 0f;
            // Exponential decay: total travelled distance = initial speed / damping
            knockbackVelocity += offset * knockbackDamping;

            if (!IsKnockedBack)
            {
                knockbackVelocity = Vector3.zero;   // Too weak to be noticed (or damping at 0): ignored
                return;
            }

            // Drop the current path so the agent doesn't steer against the knockback.
            // MoveTo() gives it a new destination once the knockback is over.
            NavMeshAgent.ResetPath();
            NavMeshAgent.velocity = Vector3.zero;
        }
        
        private void UpdateKnockback()
        {
            if (!IsKnockedBack)
                return;

            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
            {
                knockbackVelocity = Vector3.zero;
                return;
            }

            // Move() slides along the NavMesh edges instead of leaving the NavMesh
            NavMeshAgent.Move(knockbackVelocity * Time.deltaTime);
            knockbackVelocity *= Mathf.Exp(-knockbackDamping * Time.deltaTime);   // framerate independent

            if (!IsKnockedBack)
                knockbackVelocity = Vector3.zero;
        }
        
        private void OnAttackHit(in AttackHit hit)
        {
            if (hit.target is not IRuntimeNpc npc)
                return;

            Transform facing = Mesh != null ? Mesh : transform;
            Vector3 direction = PushBackExtension.GetPushBackDirection(facing.forward, hit.hitbox.PushBackMultiplier);
            npc.PushBackWithDamage(hit.hitbox.Damage, direction);
        }

        protected virtual void OnSetup()
        {
            
        }
        
        private void Unsubscribe()
        {
            if (instance == null)
                return;

            instance.OnModifyHealth -= OnModifyHealth;
            instance.OnDeath -= OnDeath;
        }
    }
}
