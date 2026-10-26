using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires
{
    public class CampPhase : Phase<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey playerKey;
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        private readonly IRuntimeArea currentArea;

        public CampPhase(CrossRoadGameModeContext gameModeContext, IRuntimeArea currentArea)
        {
            this.gameModeContext = gameModeContext;
            this.currentArea = currentArea;
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
            await Awaitable.MainThreadAsync();
            return true;
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