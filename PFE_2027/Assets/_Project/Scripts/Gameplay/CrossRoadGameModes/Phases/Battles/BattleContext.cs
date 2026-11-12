using PFE.Gameplay.Scripts.Enemy;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public struct BattleContext
    {
        public readonly BattlePhase phase;

        public BattleContext(BattlePhase phase)
        {
            this.phase = phase;
        }
    }
}