using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.Phases;
using Helteix.Tools.Phases.Listeners;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    public class NpcGenerator : MonoBehaviour, IPhaseListener<GenerateNpcPhase>, IPhaseListener<ProtectPilgrimPhase>
    {
        [SerializeField]
        private Transform container;

        private PilgrimInstance pilgrimInstance;
        private IAreaData currentArea;
        private IRuntimeArea runtimeArea;
        private NpcManager npcManager;
        private List<SpawnPoint> spawnPoints;

        private void OnEnable()
        {
            this.Register<GenerateNpcPhase>();
            this.Register<ProtectPilgrimPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<GenerateNpcPhase>();
            this.Unregister<ProtectPilgrimPhase>();
        }
        
        private void OnPilgrimPositionChanged(Vector3 position)
        {
            SpawnNpcs();
        }
        

        void IPhaseListener<GenerateNpcPhase>.OnPhaseBegin(GenerateNpcPhase phase)
        {
            container.ClearChildren();

            runtimeArea = phase.area;
            if (runtimeArea == null)
            {
                Debug.LogError("[NpcGenerator] No runtime area available, NPCs cannot be placed.", this);
                phase.SetResult(false);
                return;
            }

            currentArea = phase.currentArea;
            
            spawnPoints = new List<SpawnPoint>();
            spawnPoints.AddRange(currentArea.SpawnPoints);
            
            npcManager = phase.npcManager;
            
            InitializePilgrim();
            
            phase.SetResult(true);
        }

        void IPhaseListener<GenerateNpcPhase>.OnPhaseEnd(GenerateNpcPhase phase)
        {
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrimInstance = phase.pilgrimInstance;
            pilgrimInstance.OnPositionChanged += OnPilgrimPositionChanged;
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            phase.pilgrimInstance.OnPositionChanged -= OnPilgrimPositionChanged;
        }

        private void InitializePilgrim()
        {
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                var spawnPoint = spawnPoints[i];
                
                NpcData npcData = spawnPoint.Npc;
                if (npcData == null)
                {
                    Debug.LogWarning($"[NpcGenerator] Spawn point '{spawnPoint.AnchorId}' has no NPC assigned.", this);
                    continue;
                }
                
                if(npcData is not PilgrimData)
                    continue;

                if (!npcData.TryGet(out INpcContainer npcContainer))
                {
                    Debug.LogError($"[NpcGenerator] No INpc behaviour registered for '{npcData.GetType().Name}'.",
                        this);
                    continue;
                }

                if (!runtimeArea.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
                {
                    Debug.LogWarning(
                        $"[NpcGenerator] Spawn anchor '{spawnPoint.AnchorId}' not found in the current area.", this);
                    continue;
                }

                Transform runtimeNpc = npcData.Prefab.InstantiatePrefab();
                runtimeNpc.SetPositionAndRotation(anchor.position, anchor.rotation);
                runtimeNpc.SetParent(container);

                // the behaviour creates the instance and plugs it into the runtime (error already logged on failure)
                var context = new NpcInstanceContext(runtimeArea.Path);
                INpcInstance instance = npcContainer.CreateInstance(npcData, context, runtimeNpc);
                if (instance == null)
                {
                    Destroy(runtimeNpc.gameObject);
                    continue;
                }

                npcManager.Add(instance);
                spawnPoints.RemoveAt(i);
            }
        }
        
        private void SpawnNpcs()
        {
            for (var i = 0; i < spawnPoints.Count; i++)
            {
                var spawnPoint = spawnPoints[i];
                if (spawnPoint.Progression > pilgrimInstance.Progression)
                    return;

                NpcData npcData = spawnPoint.Npc;
                if (npcData == null)
                {
                    Debug.LogWarning($"[NpcGenerator] Spawn point '{spawnPoint.AnchorId}' has no NPC assigned.", this);
                    continue;
                }

                if (!npcData.TryGet(out INpcContainer npcContainer))
                {
                    Debug.LogError($"[NpcGenerator] No INpc behaviour registered for '{npcData.GetType().Name}'.",
                        this);
                    continue;
                }

                if (!runtimeArea.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
                {
                    Debug.LogWarning(
                        $"[NpcGenerator] Spawn anchor '{spawnPoint.AnchorId}' not found in the current area.", this);
                    continue;
                }

                Transform runtimeNpc = npcData.Prefab.InstantiatePrefab();
                runtimeNpc.SetPositionAndRotation(anchor.position, anchor.rotation);
                runtimeNpc.SetParent(container);

                // the behaviour creates the instance and plugs it into the runtime (error already logged on failure)
                var context = new NpcInstanceContext(runtimeArea.Path);
                INpcInstance instance = npcContainer.CreateInstance(npcData, context, runtimeNpc);
                if (instance == null)
                {
                    Destroy(runtimeNpc.gameObject);
                    continue;
                }

                npcManager.Add(instance);
                spawnPoints.RemoveAt(i);
            }
        }
    }
}
