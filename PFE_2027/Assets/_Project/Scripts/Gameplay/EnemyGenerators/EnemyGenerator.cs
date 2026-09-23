using System.ComponentModel;
using Codice.CM.Common;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    public class EnemyGenerator : MonoBehaviour, IPhaseListener<GenerateEnemyPhase>
    {
        [SerializeField]
        private Transform enemyPrefab;
        
        private void OnEnable()
        {
            this.Register<GenerateEnemyPhase>();
        }
        private void OnDisable()
        {
            this.Unregister<GenerateEnemyPhase>();
        }
        
        public void OnPhaseBegin(GenerateEnemyPhase phase)
        {
            enemyPrefab.ClearChildren();

            var currentEnemy = phase.currentEnemy;
                
            var runtime = currentEnemy.data.PrefabMesh.InstantiatePrefab();
            if (runtime.TryGetComponent<RuntimeEnemy>(out var runtimeEnemy))
            {
                runtimeEnemy.Setup(currentEnemy);
            }
            
            currentEnemy.Spawn();
            enemyPrefab.transform.localPosition = Vector3.zero;
            
            phase.SetResult(true);
        }
        
        public void OnPhaseEnd(GenerateEnemyPhase phase)
        {
        }

    }
}