using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Detection volume on a bone (Anchor) while its window is open: every IDamageable it touches is handed
    // once per action to the owner's hit callback (HitboxHit)
    [Serializable]
    public struct Hitbox : ITimedFlag
    {
        [field: SerializeField]
        public TimeWindow Window { get; private set; }

        [field: SerializeField, BoxGroup("Shape")]
        public HitboxAnchor Anchor { get; private set; }

        [field: SerializeField, BoxGroup("Shape")]
        public HitboxShape Shape { get; private set; }

        [field: SerializeField, BoxGroup("Shape")]
        public Vector3 Offset { get; private set; }

        [field: SerializeField, BoxGroup("Shape")]
        public Vector3 Size { get; private set; }

        [field: SerializeField, BoxGroup("Shape")]
        public Vector3 Rotation { get; private set; }

        [field: SerializeField, BoxGroup("Metrics")]
        public int Damage { get; private set; }

        // Knockback strength of this hitbox (1 = standard hit), scaled by the current strength of the hitter:
        // 1 normally, what is left of its slide while knocked back (keep it <= 1 on pushback windows so chains fade)
        [field: SerializeField, Range(0f, 10f), BoxGroup("Metrics")]
        public float Strength { get; private set; }
    }
}
