using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Dialogues;
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
        private readonly IAreaData  currentArea;
        private readonly TalkData talkData;
        
        private ChannelKey key;
        private IRuntimeArea runtimeArea;

        public DialoguePhase(CrossRoadGameModeContext gameModeContext, DialogueAreaData currentArea)
        {
            this.gameModeContext = gameModeContext;
            this.currentArea = currentArea;
            talkData = currentArea.TalkData;
        }
        
        protected override Awaitable Initialize(CancellationToken token)
        {
            Debug.Log($"[Dialogue Phase] {gameModeContext.NpcManager.Count}");
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in gameModeContext.gameMode.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.High, true);
            }
            
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
            
            while (true)
            {
                var talkPhase =  new TalkPhase(gameModeContext, talkData.Texts);
                var talkResult = await talkPhase.Run();

                //---Douane de check si pas atteint la fin---//
                if (!talkResult.value)
                    return false;

                //---Douane de check si plus de choice---//
                if (talkData.Choices.Length == 0)
                    break;

                var choicePhase = new ChoicePhase(gameModeContext, talkData.Choices);
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