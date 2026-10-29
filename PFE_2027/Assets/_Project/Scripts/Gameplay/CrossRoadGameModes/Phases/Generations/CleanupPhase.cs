using System.Threading;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    //a faire sauter => pas de clear des instance mais clear des runtimes 
    // a faire dans le NpcGenerator
    public class CleanupPhase : Phase<bool>
    {
        private readonly CrossRoadGameModeContext context;


        public CleanupPhase(CrossRoadGameModeContext context)
        {
            this.context = context;
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            NpcManager npcManager = context.NpcManager;
            npcManager.Clear();

            if (npcManager.Count != 0)
            {
                return false;
            }
            
            await Awaitable.NextFrameAsync(token);
            return true;
        }
    }
}