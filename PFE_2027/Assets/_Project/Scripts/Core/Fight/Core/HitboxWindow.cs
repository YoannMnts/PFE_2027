using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct HitboxWindow
    {
        [field: SerializeField, BoxGroup]
        public TimeWindow Window { get; private set; }
        
        [field: SerializeField, BoxGroup]
        public HitboxAnchor Anchor { get; private set; }
        
        [field: SerializeField, BoxGroup]
        public HitboxShape Shape { get; private set; }
        
        [field: SerializeField, BoxGroup]
        public Vector3 Offset { get; private set; }
        
        [field: SerializeField, BoxGroup]
        public Vector3 Size { get; private set; }
        
        [field: SerializeField, BoxGroup]
        public Vector3 Rotation { get; private set; }
        
        [field: SerializeField, BoxGroup("Metrics", false, false, -1f)]
        public int Damage { get; private set; }
        
        [field: SerializeField, Range(0f, 10f), BoxGroup("Metrics", false, false, -1f)]
        public float PushBackMultiplier { get; private set; }
    }
}