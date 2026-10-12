using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires
{
    public class CampfirePhase : Phase<bool>
    { 
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        
        private readonly CrossRoadGameModeContext gameModeContext;
        
        private ChannelKey key;
        private SceneReference sceneToLoad;
        
        private readonly IAreaData currentArea;
        
        
        public CampfirePhase(CrossRoadGameModeContext gameModeContext, IAreaData currentArea) : this(gameModeContext)
        {
            this.currentArea = currentArea;
        }
        private CampfirePhase(CrossRoadGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }


        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.Default, false);
            }
            
            return base.Initialize(token);
        }
        
        
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            //generate campfire zone
            var generateCampfirePhase = new GenerateAreaPhase(currentArea);
            var campfireResult = await generateCampfirePhase.Run();
            
            var generateNpcPhase = new GenerateNpcPhase(currentArea, campfireResult.value, gameModeContext.NpcManager);
            await generateNpcPhase.Run();

            //pilgrim + chests
            var explorationPhase = new ExplorationPhase(currentArea, gameModeContext.NpcManager);
            await explorationPhase.Run();

            //start dialogue phase ( question / answer)
            var dialoguePhase = new DialoguePhase();
            var dialogueResult = await dialoguePhase.Run();

            return dialogueResult.value;
        }
        
        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            return base.Dispose(token);
        }
    }
}