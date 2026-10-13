using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class CampfirePhase : PhaseCompletionSource<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey key;
        
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        
        public  CampfirePhase(CrossRoadGameModeContext gameModeContext, IAreaData currentArea)
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
            var explorationPhase = new CampfireExplorationPhase();
            explorationPhase.RunAndForget();
            
            var openChestPhase =  new OpenChestPhase();
            var isChestOpen = await openChestPhase.Run();

            return isChestOpen.value;
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