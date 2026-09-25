using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : MonoBehaviour, IRuntimeEnemy
    {
        private EnemyInstance enemyInstance;
        
        [SerializeField]
        private NavMeshAgent navMeshAgent;
        
        public void Setup(EnemyInstance enemy)
        {
            enemyInstance = enemy;
            enemyInstance.OnDeath += OnDeath;
            enemyInstance.OnModifyHealth += OnModifyHealth;
        }

        public void OnDestroy()
        {
            enemyInstance.OnDeath -= OnDeath;
            enemyInstance.OnModifyHealth -= OnModifyHealth;
        }

        [Button, DisableInEditorMode]
        public void DebugDamage(int damage)
        {
            Damage(damage);
        }
        
        private void OnModifyHealth(float currentHealth)
        {
            Tween.PunchScale(transform, Vector3.one * .2f, .3f);
        }

        public void MoveTo(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out var hit, 2f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
        }

        [Button, DisableInEditorMode] 
        public void DebugMoveTo(Vector3 position)
        {
            MoveTo(position);
        }
        
        public void Damage(int value)
        {
            enemyInstance.AddOrRemoveHealth(-value);
        }

        private void OnDeath()
        { 
            Destroy(gameObject);
        }
        
    }
}