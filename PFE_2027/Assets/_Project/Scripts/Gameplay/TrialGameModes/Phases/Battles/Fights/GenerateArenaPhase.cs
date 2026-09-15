using Helteix.Tools.Phases;
using PFE.Core;

namespace PFE.Gameplay.Scripts.Phases
{
    public class GenerateArenaPhase : PhaseCompletionSource<bool>
    {
        public readonly BossInstance currentBoss;

        public GenerateArenaPhase(BossInstance currentBoss)
        {
            this.currentBoss = currentBoss;
        }
    }
}