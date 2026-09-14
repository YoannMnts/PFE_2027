using System.Threading;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.GameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class ComposeBuildPhase : Phase
    {
        private readonly BattleGameModeContext context;

        public ComposeBuildPhase(BattleGameModeContext context)
        {
            this.context = context;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            var fillGroupComponent = new FillGroupComponentPhase();
            var result = await fillGroupComponent.Run();
        }
    }
}