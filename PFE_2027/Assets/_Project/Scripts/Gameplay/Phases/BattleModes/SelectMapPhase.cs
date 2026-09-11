using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class SelectMapPhase :  Phase
    {
        protected override async Awaitable ExecuteNoResult(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
        }
    }
}