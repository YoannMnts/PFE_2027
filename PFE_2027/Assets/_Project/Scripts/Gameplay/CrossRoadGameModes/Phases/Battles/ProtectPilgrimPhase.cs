using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class ProtectPilgrimPhase : PhaseCompletionSource<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        public readonly PilgrimInstance pilgrimInstance;
        private ChannelKey interactStateKey;
        

        public ProtectPilgrimPhase(CrossRoadGameModeContext context, PilgrimInstance pilgrimInstance)
        {
            this.gameModeContext = context;
            this.pilgrimInstance = pilgrimInstance;
        }
    }
}