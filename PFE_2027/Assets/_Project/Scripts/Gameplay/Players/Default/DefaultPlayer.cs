using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Formulas;
using Helteix.ChanneledProperties.Priorities;
using PFE.Core.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default
{
    public class DefaultPlayer : IPlayer, IDisposable
    {
        public event Action OnHealthModified;
        public event Action OnDeath;
        
        public PlayerData Data { get; }
        public float Health { get; private set; }
        public float MaxHealth => Data != null ? Data.MaxHealth : 0f;
        public bool IsDead { get; private set; }
        public Priority<bool> ShowUI { get; private set; }

        public DefaultPlayer(PlayerData data)
        {
            if (data == null)
                Debug.LogError("[DefaultPlayer] Created without PlayerData: assign one in the Game Metrics settings.");

            Data = data;
            Health = MaxHealth;
            ShowUI = new Priority<bool>(false);
        }
        
        public void AddOrRemoveHealth(float amount)
        {
            if (IsDead)
                return;

            // Heals never go above the max, damage never below 0
            Health = Mathf.Clamp(Health + amount, 0f, MaxHealth);
            OnHealthModified?.Invoke();
            
            if (Health <= 0f)
                Death();
        }

        public void Death()
        {
            if (IsDead)
                return;

            IsDead = true;
            OnDeath?.Invoke();
        }

        public void Dispose()
        {
        }
    }
}