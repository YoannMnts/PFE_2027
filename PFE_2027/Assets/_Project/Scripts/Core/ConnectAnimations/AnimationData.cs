using System;
using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Data synchronised with an animation clip: one list of flags, timed ones (Hitbox, Combo, Displacement...)
    // over its normalized time, the others (LoopAnimation, ChargeUp...) for the whole action.
    // The clip is played directly (ActionAnimator), it doesn't need any state in the Animator.
    public abstract class AnimationData : GameDatabaseObject
    {
        [field: SerializeField, BoxGroup("Animation")]
        public AnimationClip AnimationClip { get; private set; }

        [SerializeReference, BoxGroup("Flags")]
        private IAnimationFlag[] flags = Array.Empty<IAnimationFlag>();

        public ReadOnlySpan<IAnimationFlag> Flags => flags;

        // True if the data has a flag of this type (timed or not); gives the first one back to read its values
        public bool TryGetFlag<TFlag>(out TFlag flag) where TFlag : IAnimationFlag
        {
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i] is not TFlag compatible)
                    continue;

                flag = compatible;
                return true;
            }

            flag = default;
            return false;
        }

        // Real duration of the animation in seconds; negative = length of the clip
        public virtual float PlayDuration => -1f;

        // Real duration of the animation in seconds: PlayDuration, or the clip length
        public float PlaySeconds => PlayDuration > 0f ? PlayDuration : AnimationClip != null ? AnimationClip.length : 0f;
    }
}
