using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Entities;
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
        [SerializeField]
        private Transform container;

        private NpcManager npcManager;
        
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
            npcManager = phase.context.NpcManager;
            phase.SetResult(true);
        }

        void IPhaseListener<CleanupPhase>.OnPhaseEnd(CleanupPhase phase)
        {
        }
        
        void IPhaseListener<GenerateNpcPhase>.OnPhaseBegin(GenerateNpcPhase phase)
        {
            container.ClearChildren();
            
            //Pour récupérer l'area
            IRuntimeArea runtimeArea = phase.area;
            if (runtimeArea == null)
            {
                phase.SetResult(false);
                return;
            }

            var context = phase.gameModeContext;
            //création du context des instance
            //récupération des spawnpoints et donc des données de chaque npc
            var spawnPoints = phase.currentArea.SpawnPoints;
            
            
            // Pass 1: pilgrims, present from the start and needed by the enemies' instances
            //pour tout les spawnpoint récupérés (un par un)
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                //si un pilgrimdata est referencé au spawnpoint => on lance la méthode de spawn
                var spawnPoint = spawnPoints[i];
                
                if (spawnPoint.Npc is PilgrimData)
                {
                    Spawn(spawnPoint, runtimeArea, context);
                }
            }

            
            // Pass 2: every other NPC, placed on its anchor then hidden until the pilgrim gets close enough
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                var spawnPoint = spawnPoints[i];
                if (spawnPoint.Npc is PilgrimData)
                    continue;

                SpawnAndHide(spawnPoint, runtimeArea, context);
            }
                
            phase.SetResult(true);
        }

        void IPhaseListener<GenerateNpcPhase>.OnPhaseEnd(GenerateNpcPhase phase)
        {
        }

        private Transform SpawnAndHide(SpawnPoint spawnPoint, IRuntimeArea runtimeArea, CrossRoadGameModeContext context)
        {
            var runtime = Spawn(spawnPoint, runtimeArea, context);
            runtime?.gameObject.SetActive(false);
            return runtime;
        }
        
        private Transform Spawn(SpawnPoint spawnPoint, IRuntimeArea runtimeArea, CrossRoadGameModeContext context)
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

            // Every check passed: nothing is created before this point, so a failed check can't leave an orphan instance
            if (!runtimeNpc.TryGetComponent<IRuntimeEntity>(out var runtimeEntity))
            {
                Debug.LogError($"[NpcGenerator] '{runtimeNpc.name}' has no runtime matching the instance of '{npcData.GetType().Name}'.", this);
                Destroy(runtimeNpc.gameObject);
                return null;
            }

            if (!context.NpcManager.TryAdd(npcData, runtimeEntity, runtimeArea, spawnPoint.Progression))
            {
                Destroy(runtimeNpc.gameObject);
                return null;
            }
            
            return runtimeNpc;
        }

        // ------------------------------- PROGRESSION ---------------------------------
        
        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
        }
        
        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
        }

        // Reveals at most one NPC per pilgrim move (one per physics step): NPCs sharing a progression appear
        // on consecutive steps, which also spreads their activation cost
        private void OnPilgrimPositionChanged(float pilgrimProgression)
        {
            var query = EntityQuery.Base.WithAll<PendingNpcComponent>();
            using (ListPool<EntityAddress>.Get(out var removedEntities))
            {
                foreach (var address in npcManager.World.Query(query))
                {
                    if (!address.TryGetComponentRef<PendingNpcComponent>(out var componentRef))
                        continue;
                    
                    ref var component = ref componentRef.GetValue();

                    if (!(component.progressionGoal <= pilgrimProgression)) 
                        continue;
                    
                    var runtimeEntity = npcManager.runtimeEntityRegistry.GetRef(address.entity.id);
                    runtimeEntity.SetActive();
                    removedEntities.Add(address);
                }

                foreach (var address in removedEntities)
                {
                    address.RemoveComponent<PendingNpcComponent>();
                    address.Activate();
                }
            }
        }
    }
}
