using PFE.Core.Scripts.Databases;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    public class ComponentGroupData : GameDatabaseObject
    {
        [field: SerializeField]
        public Transform UIPrefab { get; private set; }
    }
}