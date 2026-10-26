using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;

namespace PFE.Gameplay.Scripts.Enemy
{
    // Health and events are handled by NpcInstance, only enemy-specific state goes here
    public class EnemyInstance : NpcInstance<EnemyData>
    {
        // Pilgrim this enemy goes after (null if the area has none)
        public PilgrimInstance Pilgrim { get; private set; }

        public EnemyInstance(EnemyData data, PilgrimInstance pilgrim) : base(data)
        {
            Pilgrim = pilgrim;
        }
    }
}
