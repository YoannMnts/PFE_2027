using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Area
{
    /// <summary>
    /// Spawn marker placed in a map prefab (the "where"). No runtime logic:
    /// the AreaData SpawnPoints map an enemy (the "what") to it through its Id.
    /// The Id is a copy of the GameObject name, synced in the editor: serialized to avoid
    /// allocating gameObject.name at runtime. Renaming the marker breaks the link on the data side.
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
