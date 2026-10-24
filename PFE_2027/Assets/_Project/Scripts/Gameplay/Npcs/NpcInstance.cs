using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.Interactions;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Runtime state of an NPC, TData typed to access specific fields without casting
    public abstract class NpcInstance<TData> : INpcInstance
        where TData : NpcData
    {
        public event Action<float> OnModifyHealth;
        public event Action<INpcData> OnDeath;
        
        public event Action<bool> OnModifyInteractable;

        public readonly TData data;
        INpcData INpcInstance.Data => data;

        public float CurrentHealth { get; private set; }
        
        // Raw destination given by the game (pilgrim position, spline point...)
        public Vector3 Goal { get; private set; }

        // Where the NPC actually goes: the goal after its behaviour redirected it
        public Vector3 TargetPosition { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        
        private Condition interactableCondition;
        private ChannelKey interactStateKey;

        // resolved once here rather than a lookup on every call
        private readonly INpcContainer container;

        protected NpcInstance(TData data)
        {
            this.data = data;
            CurrentHealth = data.MaxHealth;
            
            interactableCondition = new Condition(false);
            interactStateKey = ChannelKey.GetUniqueChannelKey();
            interactableCondition.AddCondition(interactStateKey, false);

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
                OnDeath?.Invoke(data);
            }
        }
        

        public void SetGoal(Vector3 goal)
        {
            Goal = goal;
            TargetPosition = container != null && !IsDead
                ? container.RedirectTargetPosition(data, this, goal)
                : goal;
        }
        
        public virtual bool TryLaunchInteractable(bool isInteractable)
        {
            OnModifyInteractable?.Invoke(isInteractable);
            interactableCondition.Write(interactStateKey, isInteractable);

            if (!interactableCondition.Value)
            {
                return false;
            }
            
            Interact();
            Debug.Log($"{this} try interact");
            return true;        
        }

        protected virtual void Interact()
        {
            //TODO comportement global
            Debug.Log($"{this} is interacted");
        }
    }
}
