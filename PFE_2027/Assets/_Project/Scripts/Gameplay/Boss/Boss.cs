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

        public void Attack(TData data)
        {
            //TODO à coder plus tard
        }

        public void TakeDamage(TData data, int damage, int currentHealth)
        {
            /* PAS SUR QUON EST BESOIN DE LA MAX EN LOGIC
            var bossMetric = data.Metrics.GetValue(0);
            var maxHealth = bossMetric.Health;
            */
            
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Dying(data);
            }
        }

        public void Dying(TData data)
        {
            
        }
    }
}
