using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Cone used to bend a knockback toward the NPC closest to its axis ("knockback into knockback").
    // Shared asset: several PushbackData can use the same cone.
    [CreateAssetMenu(menuName = "PFE/Fight/Knockback Redirect")]
    public class KnockbackRedirectData : ScriptableObject
    {
        // Length of the cone (meters)
        [field: SerializeField, Min(0f)]
        public float Range { get; private set; } = 4f;

        // Angle between the knockback direction and the edge of the cone (degrees): 30 = a 60° wide cone
        [field: SerializeField, Range(0f, 90f)]
        public float HalfAngle { get; private set; } = 30f;
    }
}
