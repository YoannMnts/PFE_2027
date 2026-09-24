using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : MonoBehaviour, IRuntimeEnemy
    {
        private EnemyInstance enemyInstance;
        
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