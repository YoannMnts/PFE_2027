using System;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Debugging._Project.Scripts.Debugging
{
    public class BattleToCampTransition : MonoBehaviour, 
        IPhaseListener<ProtectPilgrimPhase>
    {
        private ProtectPilgrimPhase protectPhase;


        private void OnEnable()
        {
            this.Register<ProtectPilgrimPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<ProtectPilgrimPhase>();        
        }


        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                protectPhase.SetResult(true);
            }
        }


        public void OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            protectPhase = phase;
        }

        public void OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            protectPhase = null;
        }
    }
}