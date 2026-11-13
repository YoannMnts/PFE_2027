using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Single mapper domain for every NPC -> generates INpcContainer
    [GenerateContainer]
    public interface INpc<in TData> : IBehaviour<TData> where TData : INpcData
    {
        [AddToContainer]
        void SetupEntity(TData data, EntitySetupContext context);
        
        [AddToContainer]
        // final destination of the NPC from the goal given by the game (pilgrim position, spline point...):
        // lets each NPC type go somewhere else than straight to it (flank, keep its distance, stay home...)
        Vector3 RedirectTargetPosition(TData data, EntityAddress address, Vector3 goal);

        [AddToContainer]
        // computes the new health (negative amount = damage)
        float ModifyHealth(TData data, int amount, float currentHealth);

        [AddToContainer]
        // called when health reaches 0
        void Dying(TData data);
    }
}
