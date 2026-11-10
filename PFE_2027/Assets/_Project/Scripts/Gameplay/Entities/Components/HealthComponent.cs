using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Formulas;
using PFE.Core.Scripts.ECS;

namespace PFE.Gameplay.Scripts.Components
{
    public struct HealthComponent : IEntityComponent
    {
        public Formula<float> Health { get; private set; }
        
        private readonly ChannelKey key;

        public HealthComponent(float maxHealth)
        {
            Health = new Formula<float>(maxHealth);
            key = ChannelKey.GetUniqueChannelKey();
        }

        public void AddOrRemove(int amount)
        {
            Health.Subtract(key, amount);
        }
    }
}