using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct HitboxWindow
    {
        [field: SerializeField]
        public TimeWindow Window { get; private set; }
        
        [field: SerializeField]
        public HitboxAnchor Anchor { get; private set; }
        
        [field: SerializeField]
        public HitboxShape Shape { get; private set; }
        
        [field: SerializeField]
        public Vector3 Offset { get; private set; }
        
        [field: SerializeField]
        public Vector3 Size { get; private set; }
        
        [field: SerializeField]
        public Vector3 Rotation { get; private set; }
        
        [field: SerializeField]
        public int Damage { get; private set; }
    }
}