using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy
{
    public abstract class EnemyData : GameDatabaseObject, IEnemyData
    {
        [field : SerializeField, BoxGroup("Description")]
        public string Name { get; private set; }
        [field :  SerializeField, Range(0f, 100f), BoxGroup ("Metric")]
        public float MaxHealth { get; private set; }
        
        
        [field : SerializeField, BoxGroup("References")]
        public Transform PrefabMesh { get; private set; }
        
    }
}
