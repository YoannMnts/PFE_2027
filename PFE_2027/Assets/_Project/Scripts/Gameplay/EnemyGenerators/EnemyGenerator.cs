using System.ComponentModel;
using Codice.CM.Common;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Enemy;
using PFE.Gameplay.Scripts.Enemy.DummyEnemy.Runtime;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using UnityEngine;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    public class EnemyGenerator : MonoPhaseListener<GenerateEnemyPhase>
    {
        [SerializeField]
        private Transform container;
        
        protected override void OnPhaseBegin(GenerateEnemyPhase phase)
        {
            container.ClearChildren();

            foreach (var spawnPoint in phase.currentArea.SpawnPoints)
            {
                EnemyData enemy = spawnPoint.Enemy;
                Vector3 enemyPosition = spawnPoint.Position;

                var runtimeEnemy = enemy.PrefabMesh.InstantiatePrefab();

                if (runtimeEnemy.TryGetComponent(out RuntimeEnemy compatible))
                {
                    var enemyInstance = new EnemyInstance(enemy);
                    
                    runtimeEnemy.position = enemyPosition;
                    runtimeEnemy.SetParent(container);
                    
                    compatible.Setup(enemyInstance);
                }

            }
            
            phase.SetResult(true);
        }

        protected override void OnPhaseEnd(GenerateEnemyPhase phase)
        {
        }

    }
}