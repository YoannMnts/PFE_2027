using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Dialogues;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class TalkPhase : PhaseCompletionSource<bool>
    { 
        private readonly CrossRoadGameModeContext gameModeContext;
        public readonly string[] texts;
        private ChannelKey key;

        public TalkPhase(CrossRoadGameModeContext gameModeContext, string[] texts)
        {
            this.gameModeContext = gameModeContext;
            this.texts = texts;
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