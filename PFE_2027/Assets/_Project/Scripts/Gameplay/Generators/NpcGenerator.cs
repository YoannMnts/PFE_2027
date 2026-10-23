using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;
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
    public class NpcGenerator : MonoBehaviour, IPhaseListener<GenerateNpcPhase>, IPhaseListener<ProtectPilgrimPhase>
    {
        // NPC already placed on its anchor, kept inactive until the pilgrim reaches its progression
        private readonly struct PendingNpc
        {
            public readonly GameObject runtime;
            public readonly float progression;

            public PendingNpc(GameObject runtime, float progression)
            {
                this.runtime = runtime;
                this.progression = progression;
            }
        }

        
        [SerializeField]
        private Transform container;

        // Ordered by progression: only the first one has to be checked when the pilgrim moves
        private readonly Queue<PendingNpc> pendingNpcs = new();
        private PilgrimInstance pilgrimInstance;

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

        // ---------------------------------------------------------------- generation

        void IPhaseListener<GenerateNpcPhase>.OnPhaseBegin(GenerateNpcPhase phase)
        {
            //on nettoie au cas où au START de la phase GenerateNpc
            container.ClearChildren();
            pendingNpcs.Clear();

            //Pour récupérer l'area
            IRuntimeArea runtimeArea = phase.area;
            if (runtimeArea == null)
            {
                Debug.LogError("[NpcGenerator] No runtime area available, NPCs cannot be placed.", this);
                phase.SetResult(false);
                return;
            }

            //création du context des instance
            var context = new NpcInstanceContext(runtimeArea.Path, phase.npcManager);
            //récupération des spawnpoints et donc des données de chaque npc
            var spawnPoints = phase.currentArea.SpawnPoints;
            
            

            // Pass 1: pilgrims, present from the start and needed by the enemies' instances
            //pour tout les spawnpoint récupérés (un par un)
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                //si un pilgrimdata est referencé au spawnpoint => on lance la méthode de spawn
                if (spawnPoints[i].Npc is PilgrimData)
                    Spawn(spawnPoints[i], runtimeArea, context);
            }

            
            // Pass 2: every other NPC, placed on its anchor then hidden until the pilgrim gets close enough
            using (ListPool<PendingNpc>.Get(out List<PendingNpc> hidden))
            {
                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    var spawnPoint = spawnPoints[i];
                    if (spawnPoint.Npc is PilgrimData)
                        continue;

                    Transform runtimeNpc = SpawnAndHide(spawnPoint, runtimeArea, context);
                    
                    if(runtimeNpc == null)
                        continue;
                    
                    hidden.Add(new PendingNpc(runtimeNpc.gameObject, spawnPoint.Progression));
                }

                hidden.Sort((a, b) => a.progression.CompareTo(b.progression));
                for (int i = 0; i < hidden.Count; i++)
                    pendingNpcs.Enqueue(hidden[i]);
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

            IRuntimeNpc runtime = runtimeNpc.GetComponent<IRuntimeNpc>();
            npcContainer.TryToSetupRuntime(npcData, npcInstance, runtime);
            
            return runtimeNpc;
        }

        // ---------------------------------------------------------------- progression

        
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
            if (!pendingNpcs.TryPeek(out PendingNpc next) || pilgrimInstance.Progression < next.progression)
                return;

            pendingNpcs.Dequeue();
            if (next.runtime != null)
                next.runtime.SetActive(true);
        }
    }
}
