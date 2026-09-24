using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Enemy;
using PFE.Gameplay.Scripts.Enemy.DummyEnemy.Runtime;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.RoadSystem;
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

            IRuntimeArea area = phase.area;
            if (area == null)
            {
                Debug.LogError("[EnemyGenerator] No runtime arena available, enemies cannot be placed.", this);
                phase.SetResult(false);
                return;
            }

            foreach (SpawnPoint spawnPoint in phase.currentArea.SpawnPoints)
            {
                EnemyData enemy = spawnPoint.Enemy;
                if (enemy == null)
                {
                    Debug.LogWarning($"[EnemyGenerator] Spawn point '{spawnPoint.AnchorId}' has no enemy assigned.", this);
                    continue;
                }

                if (!area.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
                {
                    Debug.LogWarning($"[EnemyGenerator] Spawn anchor '{spawnPoint.AnchorId}' not found in the current area.", this);
                    continue;
                }

                var runtimeEnemy = enemy.PrefabMesh.InstantiatePrefab();
                runtimeEnemy.SetPositionAndRotation(anchor.position, anchor.rotation);
                runtimeEnemy.SetParent(container);

                if (runtimeEnemy.TryGetComponent(out RuntimeEnemy compatible))
                    compatible.Setup(new EnemyInstance(enemy));
            }
            
            phase.SetResult(true);
        }

        protected override void OnPhaseEnd(GenerateEnemyPhase phase)
        {
        }

    }
}
