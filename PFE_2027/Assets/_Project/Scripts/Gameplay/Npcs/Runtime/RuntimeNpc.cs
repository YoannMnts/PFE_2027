using System;
using Helteix.ChanneledProperties.Formulas;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.Entities;
using PFE.Gameplay.Scripts.Interactions;
using PrimeTween;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    [RequireComponent(typeof(BehaviorGraphAgent))]
    public abstract partial class RuntimeNpc : MonoBehaviour, IRuntimeNpc, IRuntimeEntity
    {
        protected const string BEHAVIOR_SELF_NPC = "SelfNpc";
        protected const string BEHAVIOR_TARGET_POSITION = "TargetPosition";
        protected const string BEHAVIOR_ATTACK_DATA = "AttackData";
        
        [field : SerializeField, BoxGroup("References")]
        public NavMeshAgent NavMeshAgent { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public Transform Mesh { get; private set; }
        
        public abstract HitboxRunner HitboxRunner { get; protected set;}
        public abstract EntityAddress Address { get; protected set; }
        public abstract NpcManager NpcManager { get; protected set; }
        public void SetActive()
        {
            if(gameObject.activeSelf)
                return;
            
            gameObject.SetActive(true);
        }

        public abstract void Connect(CrossRoadGameModeContext context, EntityAddress address);
        public abstract void Disconnect();


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
        
        protected virtual void OnModifyHealth(float currentHealth)
        {
            Tween.PunchScale(transform, Vector3.one * .2f, .3f);
        }
        
        public abstract void ApplyKnockback(in KnockbackRequest request);
        public abstract void TakeDamage(int amount);
    }
}
