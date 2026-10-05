using System;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Runtime state of an NPC, TData typed to access specific fields without casting
    public abstract class NpcInstance<TData> : INpcInstance where TData : NpcData
    {
        public event Action<float> OnModifyHealth;
        public event Action<INpcInstance> OnDeath;

        public readonly TData data;
        public INpcData Data => data;

        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        // resolved once here rather than a lookup on every call
        private readonly INpcContainer container;

        protected NpcInstance(TData data)
        {
            this.data = data;
            CurrentHealth = data.MaxHealth;

            if (!data.TryGet(out container))
                Debug.LogError($"[NpcInstance] No INpc behaviour registered for '{data.GetType().Name}'.");
        }

        public void AddOrRemoveHealth(int amount)
        {
            if (IsDead || container == null)
                return;

            CurrentHealth = container.ModifyHealth(data, amount, CurrentHealth);
            OnModifyHealth?.Invoke(CurrentHealth);

            if (IsDead)
            {
                container.Dying(data);
                OnDeath?.Invoke(this);
            }
        }

        public void Act()
        {
            if (IsDead || container == null)
                return;

            container.Act(data, this);
        }
    }
}
