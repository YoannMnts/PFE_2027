using System.ComponentModel;
using Codice.CM.Common;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    public class EnemyGenerator : MonoPhaseListener<GenerateEnemyPhase>
    {
        [SerializeField]
        private Transform enemyPrefab;
        
        protected override void OnPhaseBegin(GenerateEnemyPhase phase)
        {
            //TODO spawn EnemyData.Prefab aux coord
            /*
            enemyPrefab.ClearChildren();

            var currentEnemy = phase.currentEnemy;
                
            var runtime = currentEnemy.data.PrefabMesh.InstantiatePrefab();
            if (runtime.TryGetComponent<RuntimeEnemy>(out var runtimeEnemy))
            {
                runtimeEnemy.Setup(currentEnemy);
            }
            
            currentEnemy.Spawn();
            enemyPrefab.transform.localPosition = Vector3.zero;
            
            */
            phase.SetResult(true);
        }

        protected override void OnPhaseEnd(GenerateEnemyPhase phase)
        {
        }

    }
}