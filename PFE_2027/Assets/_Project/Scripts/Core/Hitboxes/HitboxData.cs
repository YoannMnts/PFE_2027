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
        
        public ReadOnlySpan<HitboxWindow> Hitboxes => hitboxes;

        public ReadOnlySpan<AdditionalWindow> Windows => windows;

        // Real duration of the animation in seconds; negative = length of the clip
        public virtual float PlayDuration => -1f;

        // Real duration of the animation in seconds: PlayDuration, or the clip length
        public float PlaySeconds => PlayDuration > 0f ? PlayDuration : AnimationClip != null ? AnimationClip.length : 0f;
    }
}
