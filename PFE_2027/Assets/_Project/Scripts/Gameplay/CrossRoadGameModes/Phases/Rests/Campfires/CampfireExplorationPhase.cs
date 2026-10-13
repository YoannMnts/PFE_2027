using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires
{
    public class CampfireExplorationPhase : Phase<bool>
    {
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return true;
        }
    }
}