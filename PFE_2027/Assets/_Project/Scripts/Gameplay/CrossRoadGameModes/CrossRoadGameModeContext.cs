using System.Collections.Generic;
using Eflatun.SceneReference;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes
{
    public struct CrossRoadGameModeContext
    {
        //Npc manager rempli du lancement du gamemode 
        public NpcManager NpcManager => gameMode.NpcManager;
        public readonly CrossRoadGameMode gameMode;

        
        public CrossRoadGameModeContext(CrossRoadGameMode gameMode)
        {
            //associe le context à son gamemode
            this.gameMode = gameMode;
        }
    }
}