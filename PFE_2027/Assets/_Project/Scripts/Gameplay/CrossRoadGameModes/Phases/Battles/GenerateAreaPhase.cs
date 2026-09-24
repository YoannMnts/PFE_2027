using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.RoadSystem;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    // Le résultat est la map instanciée, transmise ensuite à la GenerateEnemyPhase.
    public class GenerateAreaPhase : PhaseCompletionSource<IRuntimeArea>
    {
        public readonly IAreaData areaData;

        public GenerateAreaPhase(IAreaData areaData)
        {
            this.areaData = areaData;
        }
    }
}
