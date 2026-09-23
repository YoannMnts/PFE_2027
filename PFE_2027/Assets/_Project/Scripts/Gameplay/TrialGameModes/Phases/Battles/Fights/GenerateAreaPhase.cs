using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts.Area;

namespace PFE.Gameplay.Scripts.Phases
{
    public class GenerateAreaPhase : PhaseCompletionSource<bool>
    {
        public readonly IAreaData areaData;

        public GenerateAreaPhase(IAreaData areaData)
        {
            this.areaData = areaData;
        }
    }
}