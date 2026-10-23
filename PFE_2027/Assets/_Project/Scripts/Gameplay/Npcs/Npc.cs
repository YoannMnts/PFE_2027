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
        //ABSTRACT car tous les Npc doivent dire COMMENT créer l'instance référencée
        protected abstract TInstance Create(TData data, NpcInstanceContext context);

        public INpcInstance CreateInstance(TData data, NpcInstanceContext context)
        {
            TInstance instance = Create(data, context);
            return instance;
        }

        public bool TryToSetupRuntime(TData currentData, INpcInstance currentInstance, IRuntimeNpc currentRuntimeNpc)
        {
            //la méthode s'écriera différemment si tout marche
            
            if (currentRuntimeNpc is IRuntimeNpc<TInstance> runtimeNpcInstance )
            {
                if (currentInstance is TInstance instance)
                {
                    //pas sur des 2 lignes
                    currentRuntimeNpc = runtimeNpcInstance;
                    currentInstance = instance;
                    
                    runtimeNpcInstance.Setup(instance);
                    return true;
                }
            }
            return false;
        }


        public Vector3 RedirectTargetPosition(TData data, INpcInstance instance, Vector3 goal)
            => RedirectTargetPosition(data, (TInstance)instance, goal);

        // Default: straight to the goal. Override to change where this NPC type actually goes.
        protected virtual Vector3 RedirectTargetPosition(TData data, TInstance instance, Vector3 goal) => goal;
        
        public virtual float ModifyHealth(TData data, int amount, float currentHealth)
            => Mathf.Clamp(currentHealth + amount, 0f, data.MaxHealth);

        public virtual void Dying(TData data)
        {
            
        }
    }
}
