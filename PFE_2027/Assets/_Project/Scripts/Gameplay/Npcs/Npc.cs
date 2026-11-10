using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Default behaviour shared by every NPC (stateless: the state lives in the instance)
    // TInstance is known at compile time here, so the behaviour is the one plugging the instance
    // into its IRuntimeNpc<TInstance>, without any cast
    public abstract class Npc<TData> : INpc<TData>
        where TData : NpcData
    {
        public abstract void SetupEntity(TData data, EntityAddress address);

        public abstract Vector3 RedirectTargetPosition(TData data, EntityAddress address, Vector3 goal);

        public virtual float ModifyHealth(TData data, int amount, float currentHealth)
            => Mathf.Clamp(currentHealth + amount, 0f, data.MaxHealth);

        public virtual void Dying(TData data)
        {
            
        }
    }
}
