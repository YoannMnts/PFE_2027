using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.Pilgrims;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class ProtectPilgrimPhase : PhaseCompletionSource<bool>
    {
        public readonly PilgrimInstance pilgrimInstance;

        public ProtectPilgrimPhase(PilgrimInstance pilgrimInstance)
        {
            this.pilgrimInstance = pilgrimInstance;
        }
    }
}