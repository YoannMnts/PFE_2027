using Helteix.ChanneledProperties.Formulas;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.Interactions;
using PrimeTween;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    [RequireComponent(typeof(BehaviorGraphAgent))]
    public abstract partial class RuntimeNpc : MonoBehaviour, IRuntimeNpc
    {
        protected const string BEHAVIOR_SELF_NPC = "SelfNpc";
        protected const string BEHAVIOR_TARGET_POSITION = "TargetPosition";
        protected const string BEHAVIOR_ATTACK_DATA = "AttackData";
        
        [field : SerializeField, BoxGroup("References")]
        public NavMeshAgent NavMeshAgent { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public Transform Mesh { get; private set; }
        
        public abstract HitboxRunner HitboxRunner { get; protected set;}

        // Meters travelled for a knockback of strength 1. Starts at PushbackData.Distance (built in Setup), lasting
        // modifiers (armor, stun, buffs) are channels: Multiply(key, value, orderInGroup: unique >= 1).
        public Formula<float> KnockbackDistance { get; protected set; }

        protected BehaviorGraphAgent behaviorGraphAgent;
        
        protected virtual void Awake()
        {
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            InitializeAnimation();
            
            if (NavMeshAgent != null)
                NavMeshAgent.updateRotation = Mesh == null;
        }
        
        protected virtual void Update()
        {
            UpdateKnockback();
            FaceMovement();
            UpdateAnimator();
        }
        
        protected virtual void LateUpdate()
        {
        }
        
        public virtual void MoveTo(Vector3 destination)
        {
            if (IsKnockedBack)
                return;

            if (NavMesh.SamplePosition(destination, out var hit, 2f, NavMesh.AllAreas))
            {
                NavMeshAgent.SetDestination(hit.position);
            }
        }
        
        protected void OnModifyHealth(float currentHealth)
        {
            Tween.PunchScale(transform, Vector3.one * .2f, .3f);
        }
        
        
        protected void OnDeath(INpcData data)
        {
            Tween.StopAll(onTarget: transform);
            Destroy(gameObject);
        }
        
        public abstract void ApplyKnockback(in KnockbackRequest request);
        public abstract void TakeDamage(int amount);
        
    }
}
