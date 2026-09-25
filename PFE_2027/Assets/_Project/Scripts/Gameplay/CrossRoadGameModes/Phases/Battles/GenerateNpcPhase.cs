using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.RoadSystem;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class GenerateNpcPhase : PhaseCompletionSource<bool>
    {
        public readonly IAreaData currentArea;
        public readonly IRuntimeArea area;

        public GenerateNpcPhase(IAreaData currentArea, IRuntimeArea area)
        {
            this.currentArea = currentArea;
            this.area = area;
        }
    }
}
