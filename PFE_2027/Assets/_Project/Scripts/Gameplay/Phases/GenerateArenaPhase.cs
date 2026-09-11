using System.Threading;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.ArenaSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class GenerateArenaPhase : PhaseCompletionSource<bool>
    {
        public readonly ArenaData data;

        public GenerateArenaPhase(ArenaData data)
        {
            this.data = data;
        }
    }
}