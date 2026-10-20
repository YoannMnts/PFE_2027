using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        // Knockback strength of this hitbox (1 = standard hit), scaled by the current strength of the hitter:
        // 1 normally, what is left of its slide while knocked back (keep it <= 1 on pushback windows so chains fade)
        [field: SerializeField, Range(0f, 10f)] 
        [BoxGroup("Metrics", false, false, -1f)]
        public float Strength { get; private set; }
    }
}