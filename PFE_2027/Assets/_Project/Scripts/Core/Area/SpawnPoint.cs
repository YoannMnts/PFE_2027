using PFE.Core.Scripts.Enemy;
using UnityEngine;

namespace PFE.Core.Scripts.Area
{
    [System.Serializable]
    public struct SpawnPoint
    {
        [field: SerializeField]
        public Vector3 Position { get; private set; }
        
        [field: SerializeField]
        public EnemyData Enemy  { get; private set; }
    }
}