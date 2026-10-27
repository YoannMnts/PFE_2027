using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.RoadSystem;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    // The result is the instantiated map, then passed to the GenerateNpcPhase.
    public class GenerateAreaPhase : PhaseCompletionSource<IRuntimeArea>
    {
        private readonly CrossRoadGameModeContext context;
        public readonly IAreaData areaData;

        public GenerateAreaPhase(CrossRoadGameModeContext context, IAreaData areaData)
        {
            this.context = context;
            this.areaData = areaData;
        }
    }
}
