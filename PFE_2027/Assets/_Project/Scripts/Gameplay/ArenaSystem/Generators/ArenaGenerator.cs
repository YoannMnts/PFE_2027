using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ArenaSystem
{
    public class ArenaGenerator : MonoPhaseListener<FightPhase>
    {
        [SerializeField]
        private Transform container;
        
        protected override void OnPhaseBegin(FightPhase phase)
        {
            base.OnPhaseBegin(phase);

            container.ClearChildren();
            
            var runtime = phase.CurrentBoss.StageBalances[0].ArenaPrefab.InstantiatePrefab();
            runtime.transform.SetParent(container);
        }

        protected override void OnPhaseEnd(FightPhase phase)
        {
            container.ClearChildren();
            
            base.OnPhaseEnd(phase);
        }
    }
}