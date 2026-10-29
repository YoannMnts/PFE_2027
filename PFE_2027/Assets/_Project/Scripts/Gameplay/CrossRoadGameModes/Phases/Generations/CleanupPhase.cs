using System.Threading;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    //a faire sauter => pas de clear des instance mais clear des runtimes 
    // a faire dans le NpcGenerator
    public class CleanupPhase : PhaseCompletionSource<bool>
    {
        public readonly CrossRoadGameModeContext context;
        
        public CleanupPhase(CrossRoadGameModeContext context)
        {
            this.context = context;
        }
    }
}