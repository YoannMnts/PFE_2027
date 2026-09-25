using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy
{
    public abstract class EnemyData : GameDatabaseObject, IEnemyData,  INpcData
    {
        [field : SerializeField, BoxGroup("Description")]
        public string Name { get; private set; }
        [field :  SerializeField, Range(0f, 100f), BoxGroup ("Metric")]
        public float MaxHealth { get; private set; }

        [field: SerializeField, BoxGroup("References")]
        public Transform Prefab { get; private set; }
    }
}
