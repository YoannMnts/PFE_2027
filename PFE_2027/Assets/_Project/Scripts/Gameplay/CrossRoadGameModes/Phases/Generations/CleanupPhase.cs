using System.Threading;
using Helteix.Tools.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class CleanupPhase : Phase<bool>
    {
        private readonly CrossRoadGameModeContext context;


        public CleanupPhase(CrossRoadGameModeContext context)
        {
            this.context = context;
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            foreach (var player in context.gameMode.Players)
            {
            }
            var npcManager = context.NpcManager;
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