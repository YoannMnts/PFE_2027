using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public struct AttackContext
    {
        public readonly Animator animator;
        public readonly HitboxAnchors anchors;
        public readonly IDamageable owner;
        public readonly LayerMask hitMask;
        public readonly AttackHitHandler onHit;

        public AttackContext(
            Animator animator, HitboxAnchors anchors, IDamageable owner, LayerMask hitMask, AttackHitHandler onHit)
        {
            this.animator = animator;
            this.anchors = anchors;
            this.owner = owner;
            this.hitMask = hitMask;
            this.onHit = onHit;
        }
    }
}