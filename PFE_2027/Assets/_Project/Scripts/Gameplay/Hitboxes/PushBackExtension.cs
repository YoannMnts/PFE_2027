using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public static class PushBackExtension
    { 
        public static void PushBackWithDamage<T>(this T target, int damage, Vector3 direction) 
            where T : IRuntimeNpc
        {
            target.TakeDamage(damage);
            
            PushBackNoDamage(target, direction);
        }
        
        // "direction" length = strength of the hit, multiplies the PushbackData.DistanceInTime curve of the target
        public static void PushBackNoDamage<T>(this T target, Vector3 direction)
            where T : IRuntimeNpc
        {
            target.ApplyKnockback(direction);
        }
        
        // Pushes away from the owner: flat direction from the owner to the target, whatever the owner faces.
        // "fallback" is used when both are at the same spot.
        public static Vector3 GetPushBackDirection(Vector3 ownerPosition, Vector3 targetPosition, Vector3 fallback, float strength)
        {
            Vector3 direction = targetPosition - ownerPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = fallback;
                direction.y = 0f;
            }

            return direction.normalized * strength;
        }
    }
}