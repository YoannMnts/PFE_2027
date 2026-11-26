using System;
using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Data synchronised with an animation clip: hitboxes and additional windows over its normalized time.
    // The clip is played directly (ActionAnimator), it doesn't need any state in the Animator.
    public abstract class HitboxData : GameDatabaseObject
    {
        [field: SerializeField, BoxGroup("Animation")]
        public AnimationClip AnimationClip { get; private set; }
        
        [SerializeField, BoxGroup("Windows")]
        private HitboxWindow[] hitboxes = Array.Empty<HitboxWindow>();
        
        [SerializeField, BoxGroup("Windows")]
        private AdditionalWindow[] windows = Array.Empty<AdditionalWindow>();

        // Flags of the whole action, without window (LoopAnimation, ChargeUp, ChargeTime, MovementLock...)
        [SerializeReference, BoxGroup("Flags")]
        private IDataFlag[] flags = Array.Empty<IDataFlag>();
        
        public ReadOnlySpan<HitboxWindow> Hitboxes => hitboxes;

        public ReadOnlySpan<AdditionalWindow> Windows => windows;

        // True if the data has a flag of this type (whole action, not the windows); gives it back to read its values
        public bool TryGetFlag<TFlag>(out TFlag flag) where TFlag : IHitboxFlag
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
