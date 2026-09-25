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
        // Nom de méthode en string (pas nameof) : GetAnchorIds n'existe qu'en éditeur.
        [field: SerializeField, ValueDropdown("GetAnchorIds")]
        public string AnchorId { get; private set; }

        [field: SerializeReference]
        public INpcData Npc { get; private set; }

#if UNITY_EDITOR
        // Liste les SpawnAnchor présents dans le prefab de l'AreaData qui contient ce SpawnPoint.
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
