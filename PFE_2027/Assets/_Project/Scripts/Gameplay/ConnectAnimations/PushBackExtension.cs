using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Builds every KnockbackRequest. The target turns it into meters: strength * KnockbackDistance.Value
    public static class PushBackExtension
    { 
        public static void PushBackWithDamage<T>(this T target, int damage, in KnockbackRequest request) 
            where T : IRuntimeNpc
        {
            target.TakeDamage(damage);
            
            PushBackNoDamage(target, request);
        }
        
        public static void PushBackNoDamage<T>(this T target, in KnockbackRequest request)
            where T : IRuntimeNpc
        {
            target.ApplyKnockback(request);
        }

        // One rule for every hit: strength = strength of the hitter * strength of the hitbox.
        // The hitter's strength is 1 for a regular attack (the hitbox decides alone), what is left of its slide
        // for an NPC being knocked back.
        public static KnockbackRequest FromHit(Vector3 ownerPosition, in HitboxHit hit, Vector3 fallback,
            float strengthScale = 1f, IRuntimeNpc source = null)
        {
            Vector3 direction = GetPushBackDirection(ownerPosition, hit.TargetPosition, fallback);
            return new KnockbackRequest(direction, strengthScale * hit.hitbox.Strength, source);
        }

        // Pushes away from the owner: flat direction from the owner to the target, whatever the owner faces.
        // "fallback" is used when both are at the same spot.
        public static Vector3 GetPushBackDirection(Vector3 ownerPosition, Vector3 targetPosition, Vector3 fallback)
        {
            Vector3 direction = targetPosition - ownerPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < KnockbackRequest.MIN_DIRECTION_SQR)
            {
                direction = fallback;
                direction.y = 0f;
            }

            return direction.normalized;
        }
    }
}