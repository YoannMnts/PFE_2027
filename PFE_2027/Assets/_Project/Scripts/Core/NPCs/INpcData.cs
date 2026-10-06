using PFE.Core.Scripts.DataMapping.Interfaces;
using UnityEngine;

namespace PFE.Core.Scripts.NPCs
{
    // Root of the INpc mapper domain: every NPC data (enemy, pilgrim...) goes through it
    public interface INpcData : IData
    {
        public string Name { get; }
        public float MaxHealth { get; }
        public float KnockbackDamping { get; }
        public Transform Prefab { get; }
    }
}
