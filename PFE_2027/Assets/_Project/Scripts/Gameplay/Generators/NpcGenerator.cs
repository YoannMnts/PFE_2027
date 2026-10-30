using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.EnemyGenerators
{
    // Spawns every NPC of the area up front (no Instantiate during the fight), then reveals them one by one
    // as the pilgrim progresses along its path.
    public class NpcGenerator : MonoBehaviour, 
        IPhaseListener<CleanupPhase>,
        IPhaseListener<GenerateNpcPhase>, 
        IPhaseListener<ProtectPilgrimPhase>
    {
        // NPC already placed on its anchor, kept inactive until the pilgrim reaches its progression
        private struct PendingNpc
        {
            public readonly GameObject runtime;
            public readonly float progression;
            public bool isActive;

            public PendingNpc(GameObject runtime, float progression, bool isActive =  false)
            {
                this.runtime = runtime;
                this.progression = progression;
                this.isActive = isActive;
            }
            public bool SetRuntimeActive()
            {
                isActive =  true;
                runtime.SetActive(isActive);
                return true;
            }
        }
        
        [SerializeField]
        private Transform container;

        // Ordered by progression: only the first one has to be checked when the pilgrim moves
        private readonly List<PendingNpc> pendingNpcs = new();
        
        private PilgrimInstance pilgrimInstance;

        private void OnEnable()
        {
            this.Register<CleanupPhase>();
            this.Register<GenerateNpcPhase>();
            this.Register<ProtectPilgrimPhase>();
        }

        private void OnDisable()
        {
            this.Unregister<CleanupPhase>();
            this.Unregister<GenerateNpcPhase>();
            this.Unregister<ProtectPilgrimPhase>();
        }

        // ---------------------CLEANUP & GENERATION------------------------------

        void IPhaseListener<CleanupPhase>.OnPhaseBegin(CleanupPhase phase)
        {
            phase.context.NpcManager.Clear();
            pendingNpcs.Clear();
            phase.SetResult(true);
        }

        void IPhaseListener<CleanupPhase>.OnPhaseEnd(CleanupPhase phase)
        {
        }
        
        void IPhaseListener<GenerateNpcPhase>.OnPhaseBegin(GenerateNpcPhase phase)
        { 
            //Pour récupérer l'area
            IRuntimeArea runtimeArea = phase.area;
            if (runtimeArea == null)
            {
                phase.SetResult(false);
                return;
            }

            //création du context des instance
            var context = new NpcInstanceContext(runtimeArea.Path, phase.gameModeContext.NpcManager, phase.gameModeContext.gameMode);
            //récupération des spawnpoints et donc des données de chaque npc
            var spawnPoints = phase.currentArea.SpawnPoints;
            
            
            // Pass 1: pilgrims, present from the start and needed by the enemies' instances
            //pour tout les spawnpoint récupérés (un par un)
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                //si un pilgrimdata est referencé au spawnpoint => on lance la méthode de spawn
                if (spawnPoints[i].Npc is PilgrimData)
                {
                    var runtimePilgrim = Spawn(spawnPoints[i], runtimeArea, context);
                    var pendingPilgrim = new  PendingNpc(runtimePilgrim.gameObject, spawnPoints[i].Progression, true);
                    pendingNpcs.Add(pendingPilgrim);
                }
            }

            
            // Pass 2: every other NPC, placed on its anchor then hidden until the pilgrim gets close enough
            using (ListPool<PendingNpc>.Get(out List<PendingNpc> hiddenEnemy))
            {
                //création et ajout runtime Enemy dans PendingNpc
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    var spawnPoint = spawnPoints[i];
                    if (spawnPoint.Npc is PilgrimData)
                        continue;

                    Transform runtimeEnemy = SpawnAndHide(spawnPoint, runtimeArea, context);
                    
                    if(runtimeEnemy == null)
                        continue;
                    
                    hiddenEnemy.Add(new PendingNpc(runtimeEnemy.gameObject, spawnPoint.Progression));
                }

                hiddenEnemy.Sort((a, b) => a.progression.CompareTo(b.progression));
                pendingNpcs.AddRange(hiddenEnemy);
            }

            phase.SetResult(true);
        }

        void IPhaseListener<GenerateNpcPhase>.OnPhaseEnd(GenerateNpcPhase phase)
        {
        }

        private Transform SpawnAndHide(SpawnPoint spawnPoint, IRuntimeArea runtimeArea, NpcInstanceContext context)
        {
            var runtime = Spawn(spawnPoint, runtimeArea, context);
            runtime?.gameObject.SetActive(false);
            return runtime;
        }
        
        private Transform Spawn(SpawnPoint spawnPoint, IRuntimeArea runtimeArea, NpcInstanceContext context)
        {
            //DOUANES MULTIPLES
            var npcData = spawnPoint.Npc;
            
            //si pas de data
            if (npcData == null)
            {
                Debug.LogWarning($"[NpcGenerator] Spawn point '{spawnPoint.AnchorId}' has no NPC assigned.", this);
                return null;
            }

            //si pas de behavior
            if (!npcData.TryGet(out INpcContainer npcContainer))
            {
                Debug.LogError($"[NpcGenerator] No INpc behaviour registered for '{npcData.GetType().Name}'.", this);
                return null;
            }

            //si pas d'ancre
            if (!runtimeArea.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
            {
                Debug.LogWarning($"[NpcGenerator] Spawn anchor '{spawnPoint.AnchorId}' not found in the current area.", this);
                return null;
            }

            //création du transform runtime
            Transform runtimeNpc = npcData.Prefab.InstantiatePrefab();
            runtimeNpc.SetPositionAndRotation(anchor.position, anchor.rotation);
            runtimeNpc.SetParent(container);

            //douane
            if (!context.npcManager.TryGetInstance(npcData, out INpcInstance npcInstance))
            {
                Debug.LogError($"[NpcGenerator] npcInstance was not found in dictionary");
                Destroy(runtimeNpc.gameObject);
                return null;
            }

            var gameModeContext = new CrossRoadGameModeContext(context.crossRoadGameMode);
            npcContainer.TrySetupRuntime(npcData,gameModeContext ,npcInstance, runtimeNpc);
            
            return runtimeNpc;
        }

        // ------------------------------- PROGRESSION ---------------------------------
        
        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrimInstance = phase.pilgrimInstance;
            pilgrimInstance.OnPositionChanged += OnPilgrimPositionChanged;
        }
        
        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            phase.pilgrimInstance.OnPositionChanged -= OnPilgrimPositionChanged;
            pilgrimInstance = null;
        }

        // Reveals at most one NPC per pilgrim move (one per physics step): NPCs sharing a progression appear
        // on consecutive steps, which also spreads their activation cost
        private void OnPilgrimPositionChanged(Vector3 position)
        {
            for (int i = 0; i < pendingNpcs.Count; i++)
            {
                var pendingNpc = pendingNpcs[i];
                if (pendingNpc.progression <= pilgrimInstance.Progression && pendingNpc.isActive == false)
                {
                    pendingNpc.SetRuntimeActive();
                }
            }
        }
    }
}
