using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.GameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class StartBuildPhase : Phase
    {
        private readonly TrialGameModeContext context;

        public StartBuildPhase(TrialGameModeContext context)
        {
            this.context = context;
        }

        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            var selectMap = new SelectGroupPhase(context);
            await selectMap.Run();
            
            var composeBuild = new ComposeBuildPhase(context);
            await composeBuild.Run();
        }
    }
}