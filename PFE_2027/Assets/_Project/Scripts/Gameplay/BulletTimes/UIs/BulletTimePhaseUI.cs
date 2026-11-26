using System;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Utilities.Scripts;
using UnityEngine;

namespace PFE.Gameplay.Scripts.BulletTimes.UIs
{
    public class BulletTimePhaseUI : MonoPhaseListener<BulletTimePhase>
    {
        [SerializeField] private CanvasGroup group;
        
        private BulletTimePhase currentPhase;

        
        private void Awake()
        {
            group.Hide();
        }

        protected override void OnPhaseBegin(BulletTimePhase phase)
        {
            base.OnPhaseBegin(phase);

            if (currentPhase != null)
            {
                currentPhase.Cancel();
            }
            currentPhase = phase;
            
            group.Show();
        }

        protected override void OnPhaseEnd(BulletTimePhase phase)
        {
            base.OnPhaseEnd(phase);
            
            group.Hide();
            currentPhase = null;
        }
    }
}