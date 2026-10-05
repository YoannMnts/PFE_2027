using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public static class AnimatorExtension
    {
        public static bool TryGetStateTime(this Animator animator, int layer, int stateHash, out float normalizedTime)
        {
            AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(layer);
            if (current.shortNameHash == stateHash)
            {
                normalizedTime = current.normalizedTime;
                return true;
            }

            if (animator.IsInTransition(layer))
            {
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(layer);
                if (next.shortNameHash == stateHash)
                {
                    normalizedTime = next.normalizedTime;
                    return true;
                }
            }

            normalizedTime = 0f;
            return false;
        }
    }
}