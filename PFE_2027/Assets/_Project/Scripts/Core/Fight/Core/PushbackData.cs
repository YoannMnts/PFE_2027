using PFE.Core.Scripts.Curves;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public abstract class PushbackData : HitboxData
    {
        // Meters travelled for a knockback of strength 1. Starting value of RuntimeNpc.KnockbackDistance
        [field: SerializeField, BoxGroup("Knockback"), Min(0f)]
        public float Distance { get; private set; }

        // Shape of the slide. X: seconds since the hit (the last key gives the duration of the knockback and of its
        // animation). Y: part of the distance already travelled; only the shape matters, the last key counts as 100%.
        // Should never go down (going down = sliding back toward the hitter).
        [field: SerializeField, BoxGroup("Knockback")]
        public AnimationCurve ProgressOverTime { get; private set; }

        // Optional: bends the knockback toward the NPC closest to the axis of this cone (none = straight)
        [field: SerializeField, BoxGroup("Knockback")]
        public KnockbackRedirectData Redirect { get; private set; }

        // The animation lasts exactly as long as the knockback curve
        public override float PlayDuration => ProgressOverTime.GetProgressDuration();
    }
}
