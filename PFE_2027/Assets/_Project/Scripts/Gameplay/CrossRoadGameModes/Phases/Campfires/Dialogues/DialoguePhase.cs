using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
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
        private readonly IAreaData  currentArea;

        private const float TIME_IN_DIALOGUE = 5f;

        private bool isDialogueRunning;

        public DialoguePhase(CrossRoadGameModeContext gameModeContext, IAreaData currentArea)
        {
            this.gameModeContext = gameModeContext;
            this.currentArea = currentArea;
        }
        
        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in gameModeContext.gameMode.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.Default, false);
            }
            
            isDialogueRunning = true;
            
            return base.Initialize(token);
        }
        
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            var cleanupPhase = new CleanupPhase(gameModeContext);
            await cleanupPhase.Run();
            
            var generateAreaPhase = new GenerateAreaPhase(gameModeContext, currentArea);
            var arenaResult = await generateAreaPhase.Run();
        
            var generateNpcPhase = new GenerateNpcPhase(gameModeContext, currentArea, arenaResult.value);
            await generateNpcPhase.Run();

            var finishDialogue = new FinishDialoguePhase();
            await finishDialogue.Run();
            
            return true;
            
            
            /*
            TODO UI
            while (isDialogueRunning)
            {
                var talkPhase =  new TalkPhase(gameModeContext);
                var talkResult = await talkPhase.Run();

                //---Douane de check si encore CHOICE---
                if (talkResult == false)
                {
                    isDialogueRunning = false;
                    break;
                }

                var choicePhase = new ChoicePhase(gameModeContext);
                await choicePhase.Run();
            }
            */
        }
        
        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in gameModeContext.gameMode.Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            isDialogueRunning = false;
            
            
            return base.Dispose(token);
        }
    }
}