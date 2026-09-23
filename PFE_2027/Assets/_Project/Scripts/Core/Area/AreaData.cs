using PFE.Core.Scripts.Databases;
using UnityEngine;
using Transform = UnityEngine.Transform;

namespace PFE.Core.Scripts.Area
{
    public class AreaData : GameDatabaseObject, IAreaData
    {
        [field: SerializeField]
        public Transform Prefab { get; private set; }
    }
}