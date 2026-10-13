using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Everything the owner of an attack needs to react to a hit
    public readonly struct HitboxHit
    {
        public readonly IDamageable target;
        public readonly Collider collider;
        public readonly HitboxWindow hitbox;
        public readonly Vector3 hitboxCenter;   // World position of the hitbox when it touched

        // Root position of the target (its component), the touched collider otherwise
        public Vector3 TargetPosition => target is Component component ? component.transform.position : collider.transform.position;

        public HitboxHit(IDamageable target, Collider collider, in HitboxWindow hitbox, Vector3 hitboxCenter)
        {
            this.target = target;
            this.collider = collider;
            this.hitbox = hitbox;
            this.hitboxCenter = hitboxCenter;
        }
    }

    // Called once per target and per attack (a target is never hit twice by the same attack)
    public delegate void AttackHitHandler(in HitboxHit hit);
}
