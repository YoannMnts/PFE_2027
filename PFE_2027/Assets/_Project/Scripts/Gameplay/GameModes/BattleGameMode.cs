using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.GameModes;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.GameModes
{
    public class BattleGameMode :  GameMode<bool>
    {
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            var startBuildPhase = new StartBuildPhase();
            await startBuildPhase.Run();
            
            var battlePhase = new BattlePhase();
            var result = await battlePhase.Run();
            
            return result.value;
        }
    }
}