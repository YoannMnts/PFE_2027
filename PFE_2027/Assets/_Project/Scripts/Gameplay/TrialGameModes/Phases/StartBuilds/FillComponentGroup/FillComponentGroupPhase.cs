using System.Collections.Generic;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.Phases
{
    public class FillComponentGroupPhase : PhaseCompletionSource<bool>
    {
        public readonly IEnumerable<ComponentData> selectedComponentDatas;

        public FillComponentGroupPhase(IEnumerable<ComponentData> selectedComponentDatas)
        {
            this.selectedComponentDatas = selectedComponentDatas;
        }
    }
}