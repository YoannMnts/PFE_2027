using System.ComponentModel;
using Codice.CM.Common;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.BossGenerators
{
    public class BossGenerator : MonoBehaviour, IPhaseListener<GenerateBossPhase>
    {
        [SerializeField]
        private Transform bossPrefab;
        
        private void OnEnable()
        {
            this.Register<GenerateBossPhase>();
        }
        private void OnDisable()
        {
            this.Unregister<GenerateBossPhase>();
        }
        
        public void OnPhaseBegin(GenerateBossPhase phase)
        {
            bossPrefab.ClearChildren();

            var currentBoss = phase.currentBoss;
                
            var runtime = currentBoss.data.PrefabMesh.InstantiatePrefab();
            if (runtime.TryGetComponent<RuntimeBoss>(out var runtimeBoss))
            {
                runtimeBoss.Setup(currentBoss);
            }
            
            currentBoss.Spawn();
            bossPrefab.transform.localPosition = Vector3.zero;
            
            phase.SetResult(true);
        }
        
        public void OnPhaseEnd(GenerateBossPhase phase)
        {
        }

    }
}