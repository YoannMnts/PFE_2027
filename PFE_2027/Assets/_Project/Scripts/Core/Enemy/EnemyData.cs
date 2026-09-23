using PFE.Core.Scripts;
using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.GameSettings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PFE.Core
{
    public abstract class EnemyData : GameDatabaseObject, IEnemyData
    {
        [field : SerializeField, BoxGroup("Description")]
        public string Name { get; private set; }
        
        [field : SerializeField, BoxGroup("Visual")]
        public Transform PrefabMesh { get; private set; }
    }
}
