using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // What the hitter sends to the target: where, how hard, and who sends it. No meters here: the target decides
    // how far a hit sends it (RuntimeNpc.KnockbackDistance).
    public readonly struct KnockbackRequest
    {
        public const float MIN_DIRECTION_SQR = 0.0001f;

        public readonly Vector3 direction;      // Flat and normalized (zero when invalid)
        public readonly float strength;         // 1 = standard hit, multiplies the target's KnockbackDistance
        public readonly IRuntimeNpc source;     // NPC that sent it (null for the player); its target can't push it back

        public KnockbackRequest(Vector3 direction, float strength, IRuntimeNpc source)
        {
            direction.y = 0f;
            this.direction = direction.sqrMagnitude < MIN_DIRECTION_SQR ? Vector3.zero : direction.normalized;
            this.strength = Mathf.Max(0f, strength);
            this.source = source;
        }

        public bool IsValid => strength > 0f && direction != Vector3.zero;

        // Same strength and source, new direction (used by the redirect cone)
        public KnockbackRequest WithDirection(Vector3 newDirection) => new(newDirection, strength, source);
    }
}
