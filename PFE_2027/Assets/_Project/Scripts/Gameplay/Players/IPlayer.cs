using System;
using Helteix.ChanneledProperties.Formulas;
using Helteix.ChanneledProperties.Priorities;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players
{
    public interface IPlayer
    {
        public event Action OnHealthModified;
        public event Action OnDeath;
        
        public float Health { get; }
        public float MaxHealth { get; }
        public Priority<bool> ShowUI { get; }
        
        public void AddOrRemoveHealth(float amount);

        public void Death();
    }
}
