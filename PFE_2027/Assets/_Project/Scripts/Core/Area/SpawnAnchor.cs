using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Area
{
    /// <summary>
    /// Marqueur de spawn posé dans le prefab d'une map (le "où"). Aucune logique runtime :
    /// les SpawnPoint de l'AreaData y associent un ennemi (le "quoi") via son Id.
    /// L'Id est une copie du nom du GameObject, synchronisée en éditeur : sérialisée pour éviter
    /// l'allocation de gameObject.name au runtime. Renommer le marqueur casse le lien côté data.
    /// </summary>
    public class SpawnAnchor : MonoBehaviour
    {
        [field: SerializeField, ReadOnly]
        public string Id { get; private set; }

#if UNITY_EDITOR
        public void SyncIdWithName()
        {
            if (Id == gameObject.name)
                return;

            Id = gameObject.name;
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private void OnValidate() => SyncIdWithName();

        private void OnDrawGizmos()
        {
            Vector3 position = transform.position;

            Gizmos.color = new Color(1f, 0.35f, 0.2f, 1f);
            Gizmos.DrawWireSphere(position, 0.4f);
            Gizmos.DrawLine(position, position + transform.forward);

            UnityEditor.Handles.Label(position + Vector3.up * 0.6f, Id);
        }
#endif
    }
}
