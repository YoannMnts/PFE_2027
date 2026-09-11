using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ArenaSystem
{
    public class ArenaGenerator : MonoPhaseListener<GenerateArenaPhase>
    {
        [SerializeField]
        private Transform container;
        
        protected override void OnPhaseBegin(GenerateArenaPhase phase)
        {
            base.OnPhaseBegin(phase);

            container.ClearChildren();
            
            var runtime = phase.data.ArenaPrefab.InstantiatePrefab();
            runtime.transform.SetParent(container);
            
            phase.SetResult(true);
        }
    }
}