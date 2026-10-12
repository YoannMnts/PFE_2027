using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class DialoguePhase : Phase<bool>
    {
        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return true;
        }
    }
}