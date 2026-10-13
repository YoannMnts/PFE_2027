using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.RoadSystem;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    // The result is the instantiated map, then passed to the GenerateNpcPhase.
    public class GenerateAreaPhase : PhaseCompletionSource<IRuntimeArea>
    {
        public readonly IAreaData areaData;

        public GenerateAreaPhase(IAreaData areaData)
        {
            this.areaData = areaData;
        }
    }
}
