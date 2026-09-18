using System;
using PFE.Core.Scripts;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.GameSettings;
using UnityEngine;

namespace PFE.Core
{
    public class BossInstance
    {
        public event Action<int> OnTakeDamage;
        public StageMetric<BossMetric> Metric => data.Metrics;
        
        public readonly BossData data;

        private int damage;
        private int currentHealth;

        public BossInstance(BossData data)
        {
            this.data = data;
        }

        public void Spawn()
        {
            if (data.TryGet(out IBossContainer container))
            {
                container.CanSpawn(data);
            }
        }

        public void AddOrRemoveHealth()
        {
            if (data.TryGet(out IBossContainer container))
            {
                container.TakeDamage(data, damage,  currentHealth);
                OnTakeDamage?.Invoke(currentHealth);
                
                Debug.Log("Health: " + currentHealth);
            }
        }
    }
}