using System;
using PFE.Core.Scripts.ECS;

namespace PFE.Gameplay.Scripts.Components
{
    public class HealthComponent : IEntityComponent
    {
        private readonly int maxHealth;
        private int health;
        
        public event Action OnDeath;
        public event Action OnHealthChanged;

        public HealthComponent(int maxHealth)
        {
            this.maxHealth = maxHealth;
            this.health = maxHealth;
        }

        public void AddOrRemove(int amount)
        {
            health += amount;
            OnHealthChanged?.Invoke();
            
            if(health <= 0)
                OnDeath?.Invoke();
            
        }
    }
}