using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.Phases;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : IPhaseListener<FightPhase>
    {
        private void OnEnable()
        {
            this.Register<FightPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<FightPhase>();
        }
        
        public void OnPhaseBegin(FightPhase phase)
        {
        }

        public void OnPhaseEnd(FightPhase phase)
        {
        }
    }
}