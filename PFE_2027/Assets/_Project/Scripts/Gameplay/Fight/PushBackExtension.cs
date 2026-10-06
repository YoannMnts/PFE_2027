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
        
        // "direction" length = total knockback distance; the NPC spreads it over several frames
        public static void PushBackNoDamage<T>(this T target, Vector3 direction)
            where T : IRuntimeNpc
        {
            target.ApplyKnockback(direction);
        }

        
        public static Vector3 GetPushBackDirection(Vector3 direction, float multiplier)
        {
            return direction.normalized * multiplier;
        }
    }
}