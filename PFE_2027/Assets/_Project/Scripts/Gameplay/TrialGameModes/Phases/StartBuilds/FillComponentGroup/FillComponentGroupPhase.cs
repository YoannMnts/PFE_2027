using System.Collections.Generic;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.GameModes;

namespace PFE.Gameplay.Scripts.Phases
{
    public class FillComponentGroupPhase : PhaseCompletionSource<bool>
    {
        public readonly TrialGameModeContext gameModeContext;
        public readonly IEnumerable<ComponentData> selectedComponentDatas;

        public FillComponentGroupPhase(TrialGameModeContext gameModeContext,IEnumerable<ComponentData> selectedComponentDatas)
        {
            this.gameModeContext = gameModeContext;
            this.selectedComponentDatas = selectedComponentDatas;
        }
    }
}