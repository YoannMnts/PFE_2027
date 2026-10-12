using PFE.Gameplay.Scripts.Animations;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public struct AttackContext
    {
        public readonly ActionAnimator actionAnimator;
        public readonly HitboxAnchors anchors;
        public readonly IDamageable owner;
        public readonly UnityEngine.LayerMask hitMask;
        public readonly AttackHitHandler onHit;

        public AttackContext(
            ActionAnimator actionAnimator, HitboxAnchors anchors, IDamageable owner, UnityEngine.LayerMask hitMask, AttackHitHandler onHit)
        {
            this.actionAnimator = actionAnimator;
            this.anchors = anchors;
            this.owner = owner;
            this.hitMask = hitMask;
            this.onHit = onHit;
        }
    }
}
