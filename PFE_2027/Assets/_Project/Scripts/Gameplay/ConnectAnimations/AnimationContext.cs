using PFE.Gameplay.Scripts.Animations;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Called once when a timed flag's window opens: the flag and the real duration of its window in seconds
    public delegate void WindowEnterHandler(ITimedFlag flag, float duration);

    public struct AnimationContext
    {
        public readonly ActionAnimator actionAnimator;
        public readonly HitboxAnchors anchors;
        public readonly IDamageable owner;
        public readonly UnityEngine.LayerMask hitMask;
        public readonly HitboxHitHandler onHit;
        public readonly WindowEnterHandler onWindowEnter;   // Optional

        public AnimationContext(
            ActionAnimator actionAnimator, HitboxAnchors anchors, IDamageable owner, UnityEngine.LayerMask hitMask,
            HitboxHitHandler onHit, WindowEnterHandler onWindowEnter = null)
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
