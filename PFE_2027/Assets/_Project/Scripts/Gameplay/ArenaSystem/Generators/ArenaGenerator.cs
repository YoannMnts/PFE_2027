using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ArenaSystem
{
    public class ArenaGenerator : MonoPhaseListener<BattlePhase>
    {
        [SerializeField]
        private Transform container;
        
        protected override void OnPhaseBegin(BattlePhase phase)
        {
            base.OnPhaseBegin(phase);

            container.ClearChildren();
            
            var runtime = phase.CurrentBoss.Metric.GetValue(0).ArenaPrefab.InstantiatePrefab();
            runtime.transform.SetParent(container);
        }

        protected override void OnPhaseEnd(BattlePhase phase)
        {
            container.ClearChildren();
            
            base.OnPhaseEnd(phase);
        }
    }
}