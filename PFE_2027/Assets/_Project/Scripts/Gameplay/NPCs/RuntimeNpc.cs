using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    public abstract class RuntimeNpc : MonoBehaviour
    {
        // Below this speed (m/s) the knockback is considered over
        private const float KNOCKBACK_STOP_SPEED = 0.05f;
        
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
        
        protected float knockbackDamping;
        
        private Vector3 knockbackVelocity;
        
        private float animatedSpeed;
        private float animatedSpeedVelocity;
        
        public bool IsKnockedBack => knockbackVelocity.sqrMagnitude > KNOCKBACK_STOP_SPEED * KNOCKBACK_STOP_SPEED;
        
        
        // Unity messages are protected virtual: a child class declaring one must override and call base.X(),
        // otherwise it would silently hide this one.
        protected virtual void Awake()
        {
            if (NavMeshAgent != null)
                NavMeshAgent.updateRotation = Mesh == null;
        }
        
        
        protected virtual void Update()
        {
            UpdateKnockback();
            FaceMovement();
            UpdateAnimator();
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
        
        // Pushes the NPC by "offset" (its length is the total distance), spread over several frames.
        // Several hits add up.
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
        
        public virtual void MoveTo(Vector3 destination)
        {
            // No path request while being pushed: the agent would steer against the knockback
            if (IsKnockedBack)
                return;
            
            if (NavMesh.SamplePosition(destination, out var hit, 2f, NavMesh.AllAreas))
            {
                NavMeshAgent.SetDestination(hit.position);
            }
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
    public abstract class RuntimeNpc<TInstance> : RuntimeNpc, IRuntimeNpc<TInstance> where TInstance : class, INpcInstance
    {
        protected TInstance instance;
        public TInstance Instance => instance;
        
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

            knockbackDamping = instance.Data.KnockbackDamping;

            OnSetup();
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
