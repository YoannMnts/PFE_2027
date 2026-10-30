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
        public ChoiceData[] ChoiceDatas {get ; private set;}

        public ChoicePhase(CrossRoadGameModeContext  gameModeContext, ChoiceData[] choiceDatas)
        {
            this.gameModeContext = gameModeContext;
            ChoiceDatas = choiceDatas;
        }
    }
}