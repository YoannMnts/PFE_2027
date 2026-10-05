using System.Collections.Generic;
using PFE.Core.Scripts.Enemy;
using PFE.Core.Scripts.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace PFE.Core.Scripts.Area
{
    [System.Serializable]
    public struct SpawnPoint
    {
        // Method name as a string (not nameof): GetAnchorIds only exists in the editor.
        [field: SerializeField, ValueDropdown("GetAnchorIds")]
        public string AnchorId { get; private set; }

        [field: SerializeField]
        public NpcData Npc { get; private set; }

#if UNITY_EDITOR
        // Lists the SpawnAnchors found in the prefab of the AreaData that owns this SpawnPoint.
        private static IEnumerable<string> GetAnchorIds(InspectorProperty property)
        {
            var targets = property.Tree.WeakTargets;
            if (targets.Count == 0 || targets[0] is not AreaData area || area.Prefab == null)
                yield break;

            foreach (SpawnAnchor anchor in area.Prefab.GetComponentsInChildren<SpawnAnchor>(true))
                yield return anchor.Id;
        }
#endif
    }
}
