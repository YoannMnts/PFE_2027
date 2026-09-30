using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Base des runtimes de NPC : reçoit son instance typée via Npc<TData, TInstance>.CreateInstance
    public abstract class RuntimeNpc<TInstance> : MonoBehaviour, IRuntimeNpc<TInstance> where TInstance : class, INpcInstance
    {
        protected TInstance instance;
        public TInstance Instance => instance;
        
        [field : SerializeField]
        public NavMeshAgent NavMeshAgent { get; private set; }

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

            OnSetup();
        }

        public virtual void MoveTo(Vector3 destination)
        {
            if (NavMesh.SamplePosition(destination, out var hit, 2f, NavMesh.AllAreas))
            {
                NavMeshAgent.SetDestination(hit.position);
            }
        }

        protected virtual void OnDestroy()
        {
            Unsubscribe();
        }

        public void Damage(int value)
        {
            instance?.AddOrRemoveHealth(-value);
        }

        [Button, DisableInEditorMode]
        public void DebugDamage(int damage)
        {
            Damage(damage);
        }

        protected virtual void OnSetup()
        {
            
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

        private void Unsubscribe()
        {
            if (instance == null)
                return;

            instance.OnModifyHealth -= OnModifyHealth;
            instance.OnDeath -= OnDeath;
        }
    }
}
