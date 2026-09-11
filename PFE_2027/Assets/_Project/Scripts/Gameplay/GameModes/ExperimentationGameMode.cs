using System.Threading;
using PFE.Core.Scripts.GameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.GameModes
{
    public class ExperimentationGameMode : GameMode<bool>
    {
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return true;
        }
    }
}