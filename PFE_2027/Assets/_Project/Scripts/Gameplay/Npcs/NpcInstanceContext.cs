using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.NPCs
{
    // What a behaviour may need to create the instance of its NPC
    public readonly struct NpcInstanceContext
    {
        public readonly SplineContainer splineContainer;
        public readonly NpcManager npcManager;
        public readonly CrossRoadGameMode crossRoadGameMode;

        public NpcInstanceContext(SplineContainer splineContainer, NpcManager npcManager, CrossRoadGameMode crossRoadGameMode)
        {
            this.splineContainer = splineContainer;
            this.npcManager = npcManager;
            this.crossRoadGameMode = crossRoadGameMode;
        }
    }
}
