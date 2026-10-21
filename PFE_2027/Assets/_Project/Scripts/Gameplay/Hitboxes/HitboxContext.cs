using PFE.Gameplay.Scripts.Animations;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Called once when a window with a flag opens: its flag and its real duration in seconds
    public delegate void WindowEnterHandler(IWindowFlag flag, float duration);

    public struct HitboxContext
    {
        public readonly ActionAnimator actionAnimator;
        public readonly HitboxAnchors anchors;
        public readonly IDamageable owner;
        public readonly UnityEngine.LayerMask hitMask;
        public readonly AttackHitHandler onHit;
        public readonly WindowEnterHandler onWindowEnter;   // Optional

        public HitboxContext(
            ActionAnimator actionAnimator, HitboxAnchors anchors, IDamageable owner, UnityEngine.LayerMask hitMask,
            AttackHitHandler onHit, WindowEnterHandler onWindowEnter = null)
        {
            this.actionAnimator = actionAnimator;
            this.anchors = anchors;
            this.owner = owner;
            this.hitMask = hitMask;
            this.onHit = onHit;
            this.onWindowEnter = onWindowEnter;
        }
    }
}
