using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace PFE.Core.Scripts.Area
{
    public class AreaData : GameDatabaseObject, IAreaData
    {
        [field: SerializeField, BoxGroup("References", true, false, 1f)]
        public Transform Prefab { get; private set; }
    }
}