using System;
using Helteix.ChanneledProperties.Formulas;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.BulletTimes;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Base of NPC runtimes: receives its typed instance through Npc<TData, TInstance>.CreateInstance
    public abstract class RuntimeNpc<TInstance, TData> : RuntimeNpc, IRuntimeNpc<TInstance>
        where TInstance : NpcInstance<TData>
        where TData : NpcData
    {
        [SerializeField, BoxGroup("Attacks")]
        private HitboxAnchors hitboxAnchors;
        
        [SerializeField, BoxGroup("Attacks")]
        private LayerMask hitMask;
        
        private TInstance instance;
        public TInstance Instance => instance;

        public override AnimationRunner AnimationRunner { get; protected set; }
        public override EntityAddress Address { get; protected set; }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }


        public override void Connect(CrossRoadGameModeContext context, EntityAddress address)
        {
            Disconnect();
            
            Address = address;
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_SELF_NPC, this);
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_ATTACK_DATA, Instance.data.AttackData);
            
            NavMeshAgent.speed = instance.data.Speed;
            
            var attackContext = new AnimationContext(ActionAnimator, hitboxAnchors, this, hitMask, OnAttackHit);
            AnimationRunner = new AnimationRunner(attackContext);

            // Base distance from the data; 0 when this NPC can't be pushed (no PushbackData, e.g. the pilgrim)
            PushbackData pushback = instance.data.PushbackData;
            KnockbackDistance = new Formula<float>(pushback != null ? pushback.DistancePerStrength : 0f);

            if (address.TryGetComponent<HealthComponent>(out var healthComponent))
                healthComponent.Health.OnValueChanged += OnModifyHealth; 
            
            OnSetup(context);
        }

        public override void Disconnect()
        {
            Address = default;
        }
        
        protected virtual void OnSetup(CrossRoadGameModeContext context) { }

        public void Setup(CrossRoadGameModeContext context, TInstance npcInstance)
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

            behaviorGraphAgent.SetVariableValue(BEHAVIOR_SELF_NPC, this);
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_ATTACK_DATA, Instance.data.AttackData);

            var attackContext = new AnimationContext(ActionAnimator, hitboxAnchors, this, hitMask, OnAttackHit);
            AnimationRunner = new AnimationRunner(attackContext);

            // Base distance from the data; 0 when this NPC can't be pushed (no PushbackData, e.g. the pilgrim)
            PushbackData pushback = instance.data.PushbackData;
            KnockbackDistance = new Formula<float>(pushback != null ? pushback.DistancePerStrength : 0f);
            
            OnSetup(context);
        }
        
        private void Unsubscribe()
        {
            if (instance == null)
                return;

            instance.OnModifyHealth -= OnModifyHealth;
            instance.OnDeath -= OnDeath;
        }

        protected override void Update()
        {
            LockMovementDuringAttack();
            NavMeshAgent.speed = instance.data.Speed.WithTimeScale();
            
            base.Update();
            
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (instance != null)
                behaviorGraphAgent.SetVariableValue(BEHAVIOR_TARGET_POSITION, instance.TargetPosition);

            AnimationRunner?.LateTick();
        }

        protected virtual void OnDestroy() => Unsubscribe();

        [Button, DisableInEditorMode]
        public void DebugDamage(int damage) => TakeDamage(damage);

        public override void TakeDamage(int amount) => instance?.AddOrRemoveHealth(-amount);

        private void OnAttackHit(in HitboxHit hit)
        {
            if (hit.target is not IRuntimeNpc npc || IsKnockbackSource(npc))
                return;

            KnockbackRequest request = PushBackExtension.FromHit(transform.position, hit, HitFallbackDirection, StrengthScale, this);
            npc.PushBackWithDamage(hit.hitbox.Damage, request);
        }
        
        private void LockMovementDuringAttack()
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            bool isLocked = 
                !IsKnockedBack 
                && AnimationRunner != null 
                && AnimationRunner.Has<MovementLock>(out _);
            
            NavMeshAgent.isStopped = isLocked;

            if (isLocked)
                NavMeshAgent.velocity = Vector3.zero;
        }
        
        public override void ApplyKnockback(in KnockbackRequest request)
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            // No PushbackData: this NPC can't be pushed (e.g. the pilgrim)
            PushbackData pushback = instance.data.PushbackData;
            if (pushback == null)
                return;

            KnockbackRequest final = pushback.Redirect != null
                ? request.WithDirection(RedirectKnockback(request.direction, pushback.Redirect, hitMask))
                : request;

            // The whole computation: what the hitter sends * how far this NPC goes for a strength of 1
            float distance = final.strength * KnockbackDistance.Value;
            float duration = StartKnockback(final, distance, pushback.ProgressOverTime);
            if (duration <= 0f)
                return;

            // Replaces the current attack; its animation lasts as long as the curve (PlayDuration),
            // a new hit restarts both
            AnimationRunner?.Begin(pushback);

            NavMeshAgent.ResetPath();
            NavMeshAgent.velocity = Vector3.zero;
        }
    }
}