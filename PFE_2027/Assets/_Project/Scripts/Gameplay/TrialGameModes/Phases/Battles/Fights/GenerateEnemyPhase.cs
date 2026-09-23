using Helteix.Tools.Phases;
using PFE.Core;

namespace PFE.Gameplay.Scripts.Phases
{
    public class GenerateEnemyPhase : PhaseCompletionSource<bool>
    {
        public readonly EnemyInstance currentEnemy;
        public GenerateEnemyPhase(EnemyInstance currentEnemy)
        {
            this.currentEnemy = currentEnemy;
        }
    }
}