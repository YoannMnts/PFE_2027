using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires
{
    public class CampfirePhase : Phase<bool>
    { 
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey playerKey;
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        
        private readonly IAreaData currentArea;
        
        
        public CampfirePhase(CrossRoadGameModeContext gameModeContext, IAreaData currentArea) : this(gameModeContext)
        {
            this.currentArea = currentArea;
        }

        public CampfirePhase(CrossRoadGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }


        protected override Awaitable Initialize(CancellationToken token)
        {
            playerKey = ChannelKey.GetUniqueChannelKey();
            foreach (var player in Players)
            {
                player.ShowUI.AddPriority(playerKey, PriorityTags.Default, false);
            }
            
            return base.Initialize(token);
        }
        
        
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            //generate campfire zone
            var generateCampfireAreaPhase = new GenerateAreaPhase(gameModeContext,  currentArea);
            var campfireResult = await generateCampfireAreaPhase.Run();
            
            //generate pilgrim
            var generateNpcPhase = new GenerateNpcPhase(gameModeContext, currentArea, campfireResult.value);
            await generateNpcPhase.Run();
            
            if (!gameModeContext.NpcManager.TryGetFirstData(out PilgrimData pilgrimData))
            {
                return false;
            }
            
            bool result = false;

            if (gameModeContext.NpcManager.TryGetInstance(pilgrimData, out INpcInstance npcInstance)
                && npcInstance is PilgrimInstance pilgrimInstance)
            {
                var campPhase = new CampPhase(gameModeContext, campfireResult.value);
                result = await campPhase.Run();
                
            }
            return result;
        }
        
        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in Players)
            {
                player.ShowUI.RemovePriority(playerKey);
            }
            return base.Dispose(token);
        }
    }
}