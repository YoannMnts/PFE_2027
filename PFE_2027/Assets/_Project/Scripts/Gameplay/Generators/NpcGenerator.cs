using System;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Enemy;
using PFE.Gameplay.Scripts.Enemy.DummyEnemy.Runtime;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    public class NpcGenerator : MonoPhaseListener<GenerateNpcPhase>
    {
        [SerializeField]
        private Transform container;

        //pour debug
        private RuntimePilgrim currentPilgrim;
        
        protected override void OnPhaseBegin(GenerateNpcPhase phase)
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
                var npc = spawnPoint.Npc;
                if (npc == null)
                {
                    Debug.LogWarning($"[EnemyGenerator] Spawn point '{spawnPoint.AnchorId}' has no enemy assigned.", this);
                    continue;
                }

                if (!area.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
                {
                    Debug.LogWarning($"[EnemyGenerator] Spawn anchor '{spawnPoint.AnchorId}' not found in the current area.", this);
                    continue;
                }

                var runtimeNpc = npc.Prefab.InstantiatePrefab();
                runtimeNpc.SetPositionAndRotation(anchor.position, anchor.rotation);
                runtimeNpc.SetParent(container);

                if (runtimeNpc.TryGetComponent(out RuntimeEnemy compatible))
                    //douille => à changer plus tard
                    compatible.Setup(new EnemyInstance((EnemyData)npc));

                if (runtimeNpc.TryGetComponent(out RuntimePilgrim pilgrim))
                {
                    //douille ++ => à changer plus tard
                    currentPilgrim = pilgrim;
                    pilgrim.Setup(new Pilgrim());
                }
                    
            }
            
            phase.SetResult(true);
        }

        protected override void OnPhaseEnd(GenerateNpcPhase phase)
        {
        }

        //pour debug
        private void LateUpdate()
        {
            foreach (Transform child in container)
            {
                if (child.TryGetComponent(out RuntimeEnemy runtimeEnemy))
                    runtimeEnemy.MoveTo(currentPilgrim.transform.position);
            }
        }
    }
}
