using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires
{
    public class CampfirePhase : Phase<bool>
    { 
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey playerKey;
        private ChannelKey interactStateKey;
        
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        
        private readonly IAreaData currentArea;
        public readonly PilgrimInstance pilgrimInstance;
        
        
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
            var generateCampfireAreaPhase = new GenerateAreaPhase(currentArea);
            var campfireResult = await generateCampfireAreaPhase.Run();
            //generate pilgrim
            var generatePilgrimPhase = new GenerateNpcPhase(currentArea, campfireResult.value, gameModeContext.NpcManager);
            await generatePilgrimPhase.Run();

            //TODO initialiser phase explo

            while (true)
            {
                await Awaitable.NextFrameAsync(token);
            }
            
            // dialogue phase ( question / response)
            var dialoguePhase = new DialoguePhase(gameModeContext);
            var dialogueResult = await dialoguePhase.Run();
            

            return dialogueResult.value;
        }
        
        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in Players)
            {
                player.ShowUI.RemovePriority(playerKey);
            }
            
            //modifier la variable de joueur campfire false
            return base.Dispose(token);
        }
    }
}