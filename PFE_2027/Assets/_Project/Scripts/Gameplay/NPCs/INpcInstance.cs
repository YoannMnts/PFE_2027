using System;
using PFE.Core.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Handle non générique pour pouvoir stocker toutes les instances ensemble (NpcManager)
    public interface INpcInstance
    {
        public event Action<float> OnModifyHealth;
        public event Action<INpcInstance> OnDeath;

        public INpcData Data { get; }
        public float CurrentHealth { get; }
        public bool IsDead { get; }

        public void AddOrRemoveHealth(int amount);
        public void Act();
    }
}
