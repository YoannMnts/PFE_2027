using System;
using PFE.Core;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.Enemy;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public class EnemyInstance
    {
        public event Action<float> OnModifyHealth;
        public event Action OnDeath;
        
        public readonly EnemyData data;

        private float currentHealth;

        public EnemyInstance(EnemyData data)
        {
            this.data = data;

            currentHealth = this.data.MaxHealth;
        }

        public void AddOrRemoveHealth(int damage)
        {
            if (data.TryGet(out IEnemyContainer container))
            {
                currentHealth = container.ModifyHealth(data, damage,  currentHealth);
                OnModifyHealth?.Invoke(currentHealth);

                if (currentHealth <= 0)
                {
                    container.Dying(data);
                    OnDeath?.Invoke();
                    
                }
                
                Debug.Log("Health: " + currentHealth);
            }
        }
    }
}