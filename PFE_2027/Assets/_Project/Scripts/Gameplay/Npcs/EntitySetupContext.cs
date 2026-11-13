using PFE.Core.Scripts.ECS;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Generators;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.NPCs
{
    public readonly struct EntitySetupContext
    {
        public readonly SplineContainer splineContainer;
        public readonly NpcManager npcManager;
        public readonly CrossRoadGameMode crossRoadGameMode;
        public readonly EntityAddress address;

        public EntitySetupContext(CrossRoadGameModeContext context, EntityAddress address)
        {
            splineContainer = context.spline;
            npcManager = context.NpcManager;
            crossRoadGameMode = context.gameMode;
            this.address = address;
        }
    }
}
