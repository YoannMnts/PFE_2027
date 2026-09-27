using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.RoadSystem;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class GenerateNpcPhase : PhaseCompletionSource<bool>
    {
        public readonly IAreaData currentArea;
        public readonly IRuntimeArea area;
        public readonly NpcManager npcManager;

        // si aucun manager n'est fourni la phase en crée un, récupérable ensuite via npcManager
        public GenerateNpcPhase(IAreaData currentArea, IRuntimeArea area, NpcManager npcManager)
        {
            this.currentArea = currentArea;
            this.area = area;
            this.npcManager = npcManager;
        }
    }
}
