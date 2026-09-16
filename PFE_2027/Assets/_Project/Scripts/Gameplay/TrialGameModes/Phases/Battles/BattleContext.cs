using PFE.Core;

namespace PFE.Gameplay.Scripts.Phases
{
    public struct BattleContext
    {
        public readonly BattlePhase phase;
        public readonly BossInstance instance;

        public BattleContext(BattlePhase phase, BossInstance instance)
        {
            this.phase = phase;
            this.instance = instance;
        }
    }
}