using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Base of NPC runtimes: receives its typed instance through Npc<TData, TInstance>.CreateInstance
    public abstract class RuntimeNpc<TInstance> : MonoBehaviour, IRuntimeNpc<TInstance> where TInstance : class, INpcInstance
    {
        protected TInstance instance;
        public TInstance Instance => instance;
        
        [field : SerializeField]
        public NavMeshAgent NavMeshAgent { get; private set; }

        // NPC visual: it faces the movement direction, the root never rotates
        // (its children, like a camera or a UI above the head, keep their orientation).
        // Empty = previous behaviour: the agent rotates the whole root.
        [field : SerializeField]
        public Transform Mesh { get; private set; }

        // Unity messages are protected virtual: a child class declaring one must override and call base.X(),
        // otherwise it would silently hide this one.
        protected virtual void Awake()
        {
            if (NavMeshAgent != null)
                NavMeshAgent.updateRotation = Mesh == null;
        }

        protected virtual void Update()
        {
            FaceMovement();
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
