using System;
using PFE.Core;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.Enemy;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public class EnemyInstance
    {
        public event Action<int> OnTakeDamage;
        
        public readonly EnemyData data;

        private int currentHealth;

        public EnemyInstance(EnemyData data)
        {
            this.data = data;
        }

        public void Spawn()
        {
            if (data.TryGet(out IEnemyContainer container))
            {
                container.CanSpawn(data);
            }
        }

        public void AddOrRemoveHealth(int damage)
        {
            if (data.TryGet(out IEnemyContainer container))
            {
                currentHealth = 100;
                
                container.TakeDamage(data, damage,  currentHealth);
                OnTakeDamage?.Invoke(currentHealth);
                
                
                Debug.Log("Data :" + data);
                Debug.Log("Health: " + currentHealth);
                Debug.Log("Damage: " + damage);
            }
        }
    }
}