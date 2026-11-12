using PFE.Core.Scripts.ECS;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.NPCs
{
    // What a behaviour may need to create the instance of its NPC
    public readonly struct NpcSetupContext
    {
        public readonly SplineContainer splineContainer;
        public readonly NpcManager npcManager;
        public readonly CrossRoadGameMode crossRoadGameMode;
        public readonly EntityAddress address;

        public NpcSetupContext(SplineContainer splineContainer, CrossRoadGameModeContext gameModeContext, EntityAddress address)
        {
            this.splineContainer = splineContainer;
            this.npcManager = gameModeContext.NpcManager;
            this.crossRoadGameMode = gameModeContext.gameMode;
            this.address = address;
        }
    }
}
