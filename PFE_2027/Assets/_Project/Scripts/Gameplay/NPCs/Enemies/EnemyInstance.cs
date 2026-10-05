using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Enemy
{
    // Health and events are handled by NpcInstance, only enemy-specific state goes here
    public class EnemyInstance : NpcInstance<EnemyData>
    {
        public EnemyInstance(EnemyData data) : base(data)
        {
            
            
        }
    }
}
