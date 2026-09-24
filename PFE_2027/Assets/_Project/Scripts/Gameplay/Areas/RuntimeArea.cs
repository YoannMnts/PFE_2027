using PFE.Core.Scripts.Area;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.RoadSystem
{
    public class RuntimeArea : MonoBehaviour, IRuntimeArea
    {
        [SerializeField, ReadOnly]
        private SpawnAnchor[] anchors;

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

        [Button("Refresh Anchors")]
        private void RefreshAnchors()
        {
            anchors = GetComponentsInChildren<SpawnAnchor>(true);

            // Renommer un GameObject ne déclenche pas le OnValidate de ses composants : on resynchronise ici.
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
        }
#endif
    }
}
