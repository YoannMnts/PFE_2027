using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Dialogues;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CampfireGameModes.Phases
{
    public class ChoicePhase : PhaseCompletionSource<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        public ChoiceData ChoiceData {get ; private set;}

        public ChoicePhase(CrossRoadGameModeContext  gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }

        /*
        protected override Awaitable Initialize(CancellationToken token)
        {
            
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return true;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            
        }
        */
    }
}