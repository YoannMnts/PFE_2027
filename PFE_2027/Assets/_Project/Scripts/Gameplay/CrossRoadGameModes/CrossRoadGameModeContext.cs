using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes
{
    public struct CrossRoadGameModeContext
    {
        public NpcManager NpcManager => gameMode.NpcManager;
        public readonly CrossRoadGameMode gameMode;

        public CrossRoadGameModeContext(CrossRoadGameMode gameMode)
        {
            this.gameMode = gameMode;
        }
    }
}