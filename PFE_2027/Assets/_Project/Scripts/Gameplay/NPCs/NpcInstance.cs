using System;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // État runtime d'un NPC, TData typé pour accéder aux champs spécifiques sans cast
    public abstract class NpcInstance<TData> : INpcInstance where TData : NpcData
    {
        public event Action<float> OnModifyHealth;
        public event Action<INpcInstance> OnDeath;

        public readonly TData data;
        public INpcData Data => data;

        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        // résolu une seule fois ici plutôt qu'un lookup à chaque appel
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
