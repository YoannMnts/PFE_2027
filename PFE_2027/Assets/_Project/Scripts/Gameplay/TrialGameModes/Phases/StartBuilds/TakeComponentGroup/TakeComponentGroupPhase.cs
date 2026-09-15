using System.Collections.Generic;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.Phases
{
    public class TakeComponentGroupPhase : PhaseCompletionSource<ComponentGroupData>
    {
        public readonly IEnumerable<ComponentGroupData> groupDatas;

        public TakeComponentGroupPhase(IEnumerable<ComponentGroupData> groupDatas)
        {
            this.groupDatas = groupDatas;
        }
    }
}