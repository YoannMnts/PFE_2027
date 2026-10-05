using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    public class NpcGenerator : MonoPhaseListener<GenerateNpcPhase>
    {
        [SerializeField]
        private Transform container;
        
        protected override void OnPhaseBegin(GenerateNpcPhase phase)
        {
            container.ClearChildren();

            IRuntimeArea area = phase.area;
            if (area == null)
            {
                Debug.LogError("[NpcGenerator] No runtime area available, NPCs cannot be placed.", this);
                phase.SetResult(false);
                return;
            }

            foreach (SpawnPoint spawnPoint in phase.currentArea.SpawnPoints)
            {
                NpcData npcData = spawnPoint.Npc;
                if (npcData == null)
                {
                    Debug.LogWarning($"[NpcGenerator] Spawn point '{spawnPoint.AnchorId}' has no NPC assigned.", this);
                    continue;
                }

                if (!npcData.TryGet(out INpcContainer npcContainer))
                {
                    Debug.LogError($"[NpcGenerator] No INpc behaviour registered for '{npcData.GetType().Name}'.", this);
                    continue;
                }

                if (!npcContainer.CanSpawn(npcData))
                    continue;

                if (!area.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
                {
                    Debug.LogWarning($"[NpcGenerator] Spawn anchor '{spawnPoint.AnchorId}' not found in the current area.", this);
                    continue;
                }

                Transform runtimeNpc = npcData.Prefab.InstantiatePrefab();
                runtimeNpc.SetPositionAndRotation(anchor.position, anchor.rotation);
                runtimeNpc.SetParent(container);

                // the behaviour creates the instance and plugs it into the runtime (error already logged on failure)
                var context = new NpcInstanceContext(area.Path);
                INpcInstance instance = npcContainer.CreateInstance(npcData, context, runtimeNpc);
                if (instance == null)
                {
                    Destroy(runtimeNpc.gameObject);
                    continue;
                }

                phase.npcManager.Add(instance);
            }

            phase.SetResult(true);
        }

        protected override void OnPhaseEnd(GenerateNpcPhase phase)
        {
        }
    }
}
