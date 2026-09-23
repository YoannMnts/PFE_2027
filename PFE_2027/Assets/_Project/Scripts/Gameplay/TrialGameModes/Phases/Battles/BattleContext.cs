using PFE.Core;

namespace PFE.Gameplay.Scripts.Phases
{
    public struct BattleContext
    {
        public readonly BattlePhase phase;
        public readonly EnemyInstance instance;

        public BattleContext(BattlePhase phase, EnemyInstance instance)
        {
            this.phase = phase;
            this.instance = instance;
        }
    }
}