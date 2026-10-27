using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class DialoguePhase : Phase<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey key;
        private IRuntimeArea runtimeArea;

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

                //---Douane de check si encore content---
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