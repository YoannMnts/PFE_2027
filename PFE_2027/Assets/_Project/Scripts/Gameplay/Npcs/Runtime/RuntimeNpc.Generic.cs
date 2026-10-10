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

            KnockbackDamping = instance.data.KnockbackDamping;

            behaviorGraphAgent.SetVariableValue(BEHAVIOR_SELF_NPC, this);
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_ATTACK_DATA, Instance.data.AttackData);

            var attackContext = new AttackContext(Animator, hitboxAnchors, this, hitMask, OnAttackHit);
            AttackRunner = new AttackRunner(attackContext);
            
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
            
            AttackRunner?.LateTick();
        }
        
        protected virtual void OnDestroy() => Unsubscribe();

        [Button, DisableInEditorMode]
        public void DebugDamage(int damage) => TakeDamage(damage);

        public void TakeDamage(int amount) => instance?.AddOrRemoveHealth(-amount);

        private void OnAttackHit(in AttackHit hit)
        {
            if (hit.target is not IRuntimeNpc npc)
                return;

            Transform facing = Mesh != null ? Mesh : transform;
            Vector3 direction = PushBackExtension.GetPushBackDirection(facing.forward, hit.hitbox.PushBackMultiplier);
            npc.PushBackWithDamage(hit.hitbox.Damage, direction);
        }
        
        private void LockMovementDuringAttack()
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            bool isLocked = !IsKnockedBack && AttackRunner != null && AttackRunner.Has(AttackFlags.MovementLock);
            NavMeshAgent.isStopped = isLocked;

            if (isLocked)
                NavMeshAgent.velocity = Vector3.zero;
        }
        
        public void ApplyKnockback(Vector3 offset)
        {
            if (NavMeshAgent == null || !NavMeshAgent.isOnNavMesh)
                return;

            offset.y = 0f;
            KnockbackVelocity += offset * KnockbackDamping;

            if (!IsKnockedBack)
            {
                KnockbackVelocity = Vector3.zero;
                return;
            }

            NavMeshAgent.ResetPath();
            NavMeshAgent.velocity = Vector3.zero;
        }
    }
}