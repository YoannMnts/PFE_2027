using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public abstract class PushbackData : HitboxData
    {
        // X: time in seconds, Y: distance travelled for a hit of strength 1 (scaled by the hitbox
        // PushBackMultiplier). The last key gives the duration of the knockback and of its animation.
        [field: SerializeField, BoxGroup("Knockback")]
        public AnimationCurve DistanceInTime { get; private set; }

        // The animation lasts exactly as long as the knockback curve
        public override float PlayDuration =>
            DistanceInTime != null && DistanceInTime.length > 0 ? DistanceInTime[DistanceInTime.length - 1].time : -1f;
    }
}
