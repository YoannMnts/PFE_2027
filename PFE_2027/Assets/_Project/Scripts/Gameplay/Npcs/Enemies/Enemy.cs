using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public abstract class Enemy<TData> : Npc<TData, EnemyInstance> where TData : EnemyData
    {
        // The pilgrim is spawned first by the NpcGenerator: already in the manager here
        protected override EnemyInstance Create(TData data, NpcInstanceContext context)
        {
            context.npcManager.TryGetFirst(out PilgrimInstance pilgrim);
            return new EnemyInstance(data, pilgrim);
        }

        public override void Dying(TData data)
        {
            // the runtime handles destruction through the instance's OnDeath event
            Debug.Log($"[Enemy] '{data.Name}' died.");
        }
    }
}
