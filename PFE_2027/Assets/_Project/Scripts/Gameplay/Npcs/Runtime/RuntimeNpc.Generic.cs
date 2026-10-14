using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.NPCs;
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

            behaviorGraphAgent.SetVariableValue(BEHAVIOR_SELF_NPC, this);
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_ATTACK_DATA, Instance.data.AttackData);

            var attackContext = new HitboxContext(ActionAnimator, hitboxAnchors, this, hitMask, OnAttackHit);
            HitboxRunner = new HitboxRunner(attackContext);
            
            OnSetup();
        }
        
        protected virtual void OnSetup() { }
        
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
            base.Update();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();

            if (instance != null)
                behaviorGraphAgent.SetVariableValue(BEHAVIOR_TARGET_POSITION, instance.TargetPosition);

            HitboxRunner?.LateTick();
        }

        protected virtual void OnDestroy() => Unsubscribe();

        [Button, DisableInEditorMode]
        public void DebugDamage(int damage) => TakeDamage(damage);

        public override void TakeDamage(int amount) => instance?.AddOrRemoveHealth(-amount);

        private void OnAttackHit(in HitboxHit hit)
        {
            if (hit.target is not IRuntimeNpc npc)
                return;

            Transform facing = Mesh != null ? Mesh : transform;
            float strength = hit.hitbox.PushBackMultiplier;
            Vector3 fallback = facing.forward;

            // Hit dealt by the pushback: as hard as this NPC slides (PushBackMultiplier converts its speed into a strength)
            if (IsKnockedBack && HitboxRunner != null && HitboxRunner.Current is PushbackData)
            {
                strength *= KnockbackVelocity.magnitude;
                fallback = KnockbackVelocity;
            }

            Vector3 direction = PushBackExtension.GetPushBackDirection(transform.position, hit.TargetPosition, fallback, strength);
            npc.PushBackWithDamage(hit.hitbox.Damage, direction);
        }
        
        private void LockMovementDuringAttack()
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            bool isLocked = !IsKnockedBack && HitboxRunner != null && HitboxRunner.Has(WindowFlags.MovementLock);
            NavMeshAgent.isStopped = isLocked;

            if (isLocked)
                NavMeshAgent.velocity = Vector3.zero;
        }
        
        public override void ApplyKnockback(Vector3 offset)
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            // No PushbackData: this NPC can't be pushed (e.g. the pilgrim)
            PushbackData pushback = instance.data.PushbackData;
            if (pushback == null)
                return;

            // offset: direction of the hit, its length is the strength (hitbox PushBackMultiplier)
            offset.y = 0f;
            float duration = StartKnockback(offset, offset.magnitude, pushback.DistanceInTime);
            if (duration <= 0f)
                return;

            // Replaces the current attack; its animation lasts as long as the curve (PlayDuration),
            // a new hit restarts both
            HitboxRunner?.Begin(pushback);

            NavMeshAgent.ResetPath();
            NavMeshAgent.velocity = Vector3.zero;
        }
    }
}