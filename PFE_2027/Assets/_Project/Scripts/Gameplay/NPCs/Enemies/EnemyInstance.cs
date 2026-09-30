using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Enemy
{
    // La vie et les events sont gérés par NpcInstance, ici uniquement l'état propre aux ennemis
    public class EnemyInstance : NpcInstance<EnemyData>
    {
        public EnemyInstance(EnemyData data) : base(data)
        {
            
            
        }
    }
}
