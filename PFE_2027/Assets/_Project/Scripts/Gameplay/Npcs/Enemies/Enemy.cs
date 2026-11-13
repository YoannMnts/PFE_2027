using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Enemy;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public abstract class Enemy<TData> : Npc<TData> where TData : EnemyData
    {
        public override void SetupEntity(TData data, EntitySetupContext context)
        {
            base.SetupEntity(data, context);
            
            var address = context.address;
            address.AddComponent(new HealthComponent(data.MaxHealth));
            address.AddComponent(new EnemyTagComponent(data));
            
            AddSpecificComponent(data, address);
        }

        public override Vector3 RedirectTargetPosition(TData data, EntityAddress address, Vector3 goal)
        {
            return goal;
        }

        protected virtual void AddSpecificComponent(TData data, EntityAddress address){}

        public override void Dying(TData data)
        {
            // the runtime handles destruction through the instance's OnDeath event
            Debug.Log($"[Enemy] '{data.Name}' died.");
        }
    }
}
