using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Dialogues;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Dialogues.Talks;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class TalkPhase : Phase<bool>
    { 
        public TalkData TalkData { get; private set; }

        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey key;

        public TalkPhase(CrossRoadGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in gameModeContext.gameMode.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.Default, true);
            }
            return base.Initialize(token);
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            while (true)
            {
                await Awaitable.NextFrameAsync(token);
            }
            
            if (TalkData.Choice == null)
            {
                return false;
            }
            return true;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in gameModeContext.gameMode.Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            return base.Dispose(token);
        }
    }
}