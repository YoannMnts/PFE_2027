using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Enemy;
using PFE.Core.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Components
{
    public struct EnemyTagComponent : IEntityComponent
    {
        public readonly EnemyData data;
        public EnemyTagComponent(NpcData data)
        {
            if (data is EnemyData enemyData)
                this.data = enemyData;

            this.data = null;
        }
    }
}