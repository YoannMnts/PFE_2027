using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class StartBuildPhase :  Phase
    {
        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            var selectMap = new SelectMapPhase();
            await selectMap.Run();
            
            var composeBuild = new ComposeBuildPhase();
            await composeBuild.Run();
        }
    }
}