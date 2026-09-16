using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.GameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class SelectGroupPhase : Phase
    {
        private readonly TrialGameModeContext context;
        private ChannelKey key;

        public SelectGroupPhase(TrialGameModeContext context)
        {
            this.context = context;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in context.trialGameMode.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.High, true);
            }
            return base.Initialize(token);
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            
            
            var componentGroupDatas =
                GameController.GameDatabase.GetAll<ComponentGroupData>();
            
            var takeGroupPhase = new TakeComponentGroupPhase(componentGroupDatas);
            var result = await takeGroupPhase.Run();
            
            foreach (var player in context.trialGameMode.Players)
            { 
                player.SetupComponentGroup(result.value);
            }
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in context.trialGameMode.Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            return base.Dispose(token);
        }
    }
}