using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class DialoguePhase : Phase<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey key;

        public DialoguePhase(CrossRoadGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }
        
        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in gameModeContext.gameMode.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.Default, false);
            }
            
            return base.Initialize(token);
        }
        
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            while (true)
            {
                var talkPhase =  new TalkPhase(gameModeContext);
                var talkResult = await talkPhase.Run();

                if (talkResult == false)
                    break;
                
                var choicePhase = new ChoicePhase(gameModeContext);
                await choicePhase.Run();
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