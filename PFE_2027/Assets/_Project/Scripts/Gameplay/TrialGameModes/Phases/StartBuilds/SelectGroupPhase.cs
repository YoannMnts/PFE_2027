using System.Threading;
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

        public SelectGroupPhase(TrialGameModeContext context)
        {
            this.context = context;
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
    }
}