using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.NPCs
{
    public abstract class NpcData : GameDatabaseObject, INpcData
    {
        [field: SerializeField, BoxGroup("References")]
        public Transform Prefab { get; private set; }
    }
}