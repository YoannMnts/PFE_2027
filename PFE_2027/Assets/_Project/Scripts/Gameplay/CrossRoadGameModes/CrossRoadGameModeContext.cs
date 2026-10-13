using System.Collections.Generic;
using Eflatun.SceneReference;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;

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