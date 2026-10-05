using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Default behaviour shared by every NPC (stateless: the state lives in the instance)
    // TInstance is known at compile time here, so the behaviour is the one plugging the instance
    // into its IRuntimeNpc<TInstance>, without any cast
    public abstract class Npc<TData, TInstance> : INpc<TData>
        where TData : NpcData
        where TInstance : class, INpcInstance
    {
        protected abstract TInstance Create(TData data, NpcInstanceContext context);

        public INpcInstance CreateInstance(TData data, NpcInstanceContext context, Transform runtime)
        {
            if (!runtime.TryGetComponent(out IRuntimeNpc<TInstance> runtimeNpc))
            {
                Debug.LogError($"[Npc] '{runtime.name}' has no IRuntimeNpc<{typeof(TInstance).Name}> on its root (data '{data.name}').", runtime);
                return null;
            }

            TInstance instance = Create(data, context);
            runtimeNpc.Setup(instance);
            return instance;
        }

        public virtual bool CanSpawn(TData data) => true;

        public void Act(TData data, INpcInstance instance) => Act(data, (TInstance)instance);

        protected abstract void Act(TData data, TInstance instance);

        public virtual float ModifyHealth(TData data, int amount, float currentHealth)
            => Mathf.Clamp(currentHealth + amount, 0f, data.MaxHealth);

        public virtual void Dying(TData data)
        {
            
        }
    }
}
