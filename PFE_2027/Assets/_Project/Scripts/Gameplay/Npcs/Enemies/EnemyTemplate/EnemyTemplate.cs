using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Enemy.Dummy;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public partial class EnemyTemplate : Enemy<EnemyTemplateData>
    {
        protected override void AddSpecificComponent(EnemyTemplateData data, EntityAddress address)
        {
            base.AddSpecificComponent(data, address);
            address.AddComponent<HealthComponent>();
        }
    }
}
