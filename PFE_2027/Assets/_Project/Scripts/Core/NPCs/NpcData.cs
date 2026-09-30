using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.NPCs
{
    // Base commune pour pouvoir référencer n'importe quelle data de NPC dans un champ sérialisé (SpawnPoint...)
    public abstract class NpcData : GameDatabaseObject, INpcData
    {
        [field: SerializeField, BoxGroup("Description")]
        public string Name { get; private set; }

        [field: SerializeField, Range(0f, 100f), BoxGroup("Metrics")]
        public float MaxHealth { get; private set; }

        [field: SerializeField, BoxGroup("References")]
        public Transform Prefab { get; private set; }
    }
}
