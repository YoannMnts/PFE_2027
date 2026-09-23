using PFE.Gameplay.Scripts.Enemy;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
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