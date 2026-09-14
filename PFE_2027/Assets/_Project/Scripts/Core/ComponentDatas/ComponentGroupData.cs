using PFE.Core.Scripts.Databases;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    [CreateAssetMenu(menuName = "PFE/ComponentSystem/ComponentGroup", fileName = "ComponentGroupData")]
    public class ComponentGroupData : GameDatabaseObject
    {
        [field: SerializeField]
        public Transform UIPrefab { get; private set; }
    }
}