using PFE.Core.Scripts.Enemy.Attacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Fight.Dashes
{
    // Dash of a character: played like an attack (animation, windows). Its movement comes from a Displacement window,
    // a Combo window can chain it into an attack.
    [CreateAssetMenu(menuName = "PFE/Fight/Dash")]
    public class DashData : HitboxData
    {
        // Real duration of the dash in seconds (the clip is sped up or slowed down), 0 = length of the clip
        [field: SerializeField, BoxGroup("Dash"), Min(0f)]
        public float Duration { get; private set; }

        // Time (in seconds) after the end of the dash before the next one is allowed
        [field: SerializeField, BoxGroup("Dash"), Min(0f)]
        public float Cooldown { get; private set; } = 0.4f;

        public override float PlayDuration => Duration > 0f ? Duration : -1f;
    }
}
