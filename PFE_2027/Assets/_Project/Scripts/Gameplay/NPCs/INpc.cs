using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Single mapper domain for every NPC -> generates INpcContainer
    [GenerateContainer]
    public interface INpc<in TData> : IBehaviour<TData> where TData : INpcData
    {
        [AddToContainer]
        // creates the instance (runtime state) specific to this NPC type and plugs it into its spawned runtime
        // returns null if the runtime doesn't match the instance type
        INpcInstance CreateInstance(TData data, NpcInstanceContext context, Transform runtime);

        [AddToContainer]
        // allows the spawn or not
        bool CanSpawn(TData data);

        [AddToContainer]
        // active behaviour of the NPC (enemy => attacks, pilgrim => walks...)
        void Act(TData data, INpcInstance context);

        [AddToContainer]
        // computes the new health (negative amount = damage)
        float ModifyHealth(TData data, int amount, float currentHealth);

        [AddToContainer]
        // called when health reaches 0
        void Dying(TData data);
    }
}
