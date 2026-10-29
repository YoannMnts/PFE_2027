using System.Collections.Generic;
using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class GenerateNpcPhase : PhaseCompletionSource<bool>
    {
        public readonly IAreaData currentArea;
        public readonly IRuntimeArea area;
        public readonly CrossRoadGameModeContext gameModeContext;

        public GenerateNpcPhase(CrossRoadGameModeContext gameModeContext, IAreaData currentArea, IRuntimeArea area)
        {
            this.currentArea = currentArea;
            this.area = area;
            this.gameModeContext = gameModeContext;
        }
        

        protected override Awaitable Initialize(CancellationToken token)
        {
            foreach (SpawnPoint spawnPoints in currentArea.SpawnPoints)
            {
                var currentNpcData = spawnPoints.Npc;
                
                var context = new NpcInstanceContext(area.Path, gameModeContext.NpcManager, gameModeContext.gameMode);
                
                if (!gameModeContext.NpcManager.TryGetInstance(currentNpcData, out _))
                {
                    gameModeContext.NpcManager.Add(currentNpcData, context);
                }
            }
            return base.Initialize(token);
        }
    }
}
