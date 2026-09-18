using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ArenaSystem
{
    public class ArenaGenerator : MonoBehaviour, IPhaseListener<GenerateArenaPhase>, IPhaseListener<FightPhase>
    {
        [SerializeField]
        private Transform container;

        private void OnEnable()
        {
            this.Register<GenerateArenaPhase>();
            this.Register<FightPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<GenerateArenaPhase>();
            this.Unregister<FightPhase>();
        }

        public void OnPhaseBegin(GenerateArenaPhase phase)
        {
            container.ClearChildren();
            
            Transform runtime = phase.currentBoss.Metric.GetValue(0).ArenaPrefab.InstantiatePrefab();
            runtime.transform.SetParent(container);
            
            phase.SetResult(true);
        }

        public void OnPhaseEnd(GenerateArenaPhase phase)
        {
        }

        public void OnPhaseBegin(FightPhase phase)
        {
        }

        public void OnPhaseEnd(FightPhase phase)
        {
            container.ClearChildren();
        }
    }
}