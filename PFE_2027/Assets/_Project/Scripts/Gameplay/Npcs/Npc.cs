using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
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

        public bool TrySetupRuntime(TData data, CrossRoadGameModeContext context, INpcInstance currentInstance, Transform prefab)
        {
            //on fait les douanes et si tous passe on peut return true
            if(!prefab.TryGetComponent<IRuntimeNpc<TInstance>>(out var runtimeNpc))
                return false;
            
            if(currentInstance is not TInstance instance)
                return false;
            
            runtimeNpc.Setup(context, instance);
            return true;
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
