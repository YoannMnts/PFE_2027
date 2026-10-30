using PFE.Core.Scripts.Area;
using PFE.Gameplay.Scripts.Players.Default;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public class RuntimeArea : MonoBehaviour, IRuntimeArea
    {
        [SerializeField, ReadOnly, BoxGroup("Spawn Anchors")]
        private SpawnAnchor[] anchors;

        [SerializeField, BoxGroup("Spawn Anchors")]
        private PlayerSpawn playerSpawn;
        
        public Transform SpawnPosition => playerSpawn != null ? playerSpawn.transform : null;

        [field: SerializeField, BoxGroup("References")]
        public SplineContainer Path { get; private set; }

        public bool TryGetAnchor(string id, out Transform anchor)
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                if (anchors[i].Id == id)
                {
                    anchor = anchors[i].transform;
                    return true;
                }
            }

            anchor = null;
            return false;
        }

#if UNITY_EDITOR
        private void OnValidate() => RefreshAnchors();

        [Button("Refresh Anchors"), BoxGroup("Spawn Anchors")]
        private void RefreshAnchors()
        {
            anchors = GetComponentsInChildren<SpawnAnchor>(true);

            // Renaming a GameObject doesn't trigger its components' OnValidate: resync here.
            for (int i = 0; i < anchors.Length; i++)
                anchors[i].SyncIdWithName();

            for (int i = 0; i < anchors.Length; i++)
            {
                for (int j = i + 1; j < anchors.Length; j++)
                {
                    if (anchors[i].Id == anchors[j].Id)
                        Debug.LogWarning($"[RuntimeArea] Duplicate spawn anchor id '{anchors[i].Id}' in '{name}'.", this);
                }
            }

            RefreshPlayerSpawn();
        }

        private void RefreshPlayerSpawn()
        {
            PlayerSpawn[] spawns = GetComponentsInChildren<PlayerSpawn>(true);
            playerSpawn = spawns.Length > 0 ? spawns[0] : null;

            if (spawns.Length == 0)
                Debug.LogWarning($"[RuntimeArea] No PlayerSpawn in '{name}': the player has no arrival point in this area.", this);
            else if (spawns.Length > 1)
                Debug.LogWarning($"[RuntimeArea] {spawns.Length} PlayerSpawn in '{name}': only '{spawns[0].name}' is used.", this);
        }
#endif
    }
}