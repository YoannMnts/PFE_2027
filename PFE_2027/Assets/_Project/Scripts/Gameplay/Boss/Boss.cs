using PFE.Core.Scripts.AIPattern;
using PFE.Core.Scripts.Attacks;
using UnityEngine;

namespace PFE.Core
{
    public abstract class Boss<TData> : IBoss<TData> where TData : BossData
    {
        public bool CanSpawn(TData data)
        {
             return true;
        }

        public abstract void Attack(TData data);

        public void TakeDamage(TData data, int damage, int currentHealth)
        {
            var bossMetric = data.Metrics.GetValue(0);
            var maxHealth = bossMetric.Health;

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
