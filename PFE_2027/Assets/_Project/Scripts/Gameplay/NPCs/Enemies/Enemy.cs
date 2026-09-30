using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public abstract class Enemy<TData> : Npc<TData, EnemyInstance> where TData : EnemyData
    {
        protected override EnemyInstance Create(TData data, NpcInstanceContext context) => new EnemyInstance(data);

        public override void Dying(TData data)
        {
            //le runtime gère la destruction via l'event OnDeath de l'instance
            Debug.Log($"[Enemy] '{data.Name}' died.");
        }
    }
}
