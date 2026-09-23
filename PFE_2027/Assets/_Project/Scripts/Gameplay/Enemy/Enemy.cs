using PFE.Core.Scripts.AIPattern;
using PFE.Core.Scripts.Attacks;
using UnityEngine;

namespace PFE.Core
{
    public abstract class Enemy<TData> : IEnemy<TData> where TData : EnemyData
    {
        public bool CanSpawn(TData data)
        {
             return true;
        }

        public abstract void Attack(TData data);

        public void TakeDamage(TData data, int damage, int currentHealth)
        {
            var enemyMetric = data.Metrics.GetValue(0);
            var maxHealth = enemyMetric.Health;

            currentHealth = Mathf.Clamp(currentHealth + damage, 0, maxHealth);
            
            if (currentHealth <= 0)
            {
                Dying(data);
            }
        }

        public void Dying(TData data)
        {
            //le runtime fera remonter l'info quand il faudra mourir
            return;
        }
    }
}
