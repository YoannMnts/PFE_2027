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
    public abstract partial class RuntimeNpc : MonoBehaviour
    {
        protected const string BEHAVIOR_SELF_NPC = "SelfNpc";
        protected const string BEHAVIOR_TARGET_POSITION = "TargetPosition";
        protected const string BEHAVIOR_ATTACK_DATA = "AttackData";
        
        [field : SerializeField, BoxGroup("References")]
        public NavMeshAgent NavMeshAgent { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public Transform Mesh { get; private set; }
        
        protected abstract Vector3 TargetPosition { get;  }
        public AttackRunner AttackRunner { get; protected set;}
        
        protected BehaviorGraphAgent behaviorGraphAgent;
        
        protected virtual void Awake()
        {
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
            
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
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_TARGET_POSITION, TargetPosition);
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
        
        protected virtual void OnDeath(INpcInstance deadInstance)
        {
            Tween.StopAll(onTarget: transform);
            Destroy(gameObject);
        }
    }
}
