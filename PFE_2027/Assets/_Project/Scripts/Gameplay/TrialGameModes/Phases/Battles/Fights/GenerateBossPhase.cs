using Helteix.Tools.Phases;
using PFE.Core;

namespace PFE.Gameplay.Scripts.Phases
{
    public class GenerateBossPhase : PhaseCompletionSource<bool>
    {
        public readonly BossInstance currentBoss;
        public GenerateBossPhase(BossInstance currentBoss)
        {
            this.currentBoss = currentBoss;
        }
    }
}