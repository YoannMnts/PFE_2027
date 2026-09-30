using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Comportement par défaut partagé par tous les NPC (stateless : l'état est dans l'instance)
    // TInstance est connu à la compilation ici, c'est donc le behaviour qui branche l'instance
    // sur son IRuntimeNpc<TInstance>, sans aucun cast
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
