using System.Threading;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.GameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class SelectMapPhase : Phase
    {
        private readonly BattleGameModeContext context;

        public SelectMapPhase(BattleGameModeContext context)
        {
            this.context = context;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            //TODO : PhaseCompletionSource qui renvoie un ComponentGroupData
            /*
             foreach (var player in context.battleGameMode.Players)
             {
                 player.SetupComponentGroup(data);
             }
            */
        }
    }
}