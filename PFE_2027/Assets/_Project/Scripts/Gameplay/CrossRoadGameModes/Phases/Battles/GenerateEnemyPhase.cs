using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class GenerateEnemyPhase : PhaseCompletionSource<bool>
    {
        public readonly IAreaData currentArea;

        public GenerateEnemyPhase(IAreaData currentArea)
        {
            this.currentArea = currentArea;
        }
    }
}