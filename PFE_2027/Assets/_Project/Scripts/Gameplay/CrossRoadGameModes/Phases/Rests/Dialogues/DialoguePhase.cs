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
        private readonly DialogueData dialogueData;
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey key;
        
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;

        public DialoguePhase()
        {
            
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
            while (true)
            {
                var responsePhase =  new ResponsePhase();
                var talkResult = await responsePhase.Run();
                
                if(talkResult == false)
                    break;
                
                var questionPhase = new QuestionPhase();
                await questionPhase.Run();
            }

            return true;
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