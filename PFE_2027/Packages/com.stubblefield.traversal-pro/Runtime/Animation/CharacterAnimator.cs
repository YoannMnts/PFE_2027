using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.Serialization;
using static TraversalPro.Utility;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TraversalPro
{
    /// <summary>
    /// Assigns animation values from other Character Flow components to an <see cref="Animator"/> component. 
    /// </summary>
    [AddComponentMenu("Traversal Pro/Animation/Character Animator")]
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        [Tooltip("The ICharacterMotor component for the animated character.")]
        public InterfaceRef<ICharacterMotor> characterMotor;
        [Tooltip("The IJump component for the animated character.")]
        public InterfaceRef<IJump> jump;
        [Tooltip("The name of the grounded bool parameter in the animator controller asset.")]
        [SerializeField] string groundedName = "Grounded";
        [Tooltip("The name of the free fall duration float parameter in the animator controller asset.")]
        [SerializeField] string freeFallDurationName = "FreeFallDuration";
        [Tooltip("The name of the left/right velocity float parameter in the animator controller asset.")]
        [SerializeField] string velocityXName = "VelocityX";
        [Tooltip("The name of the forward/backward velocity float parameter in the animator controller asset.")]
        [SerializeField] string velocityYName = "VelocityY";
        [Tooltip("The name of the animation speed float parameter in the animator controller asset. This will be used to speed up " +
                 "or slow down the running animation to match the character's run speed.")]
        [SerializeField] string animationSpeedName = "AnimationSpeed";
        [Tooltip("Converts character movement speed to animation run values. Any Y values greater than one will cause " +
                 "the run animation to play back at a faster speed.")]
        public AnimationCurve runSpeedToValue = GetDefaultUnityRobotCurve();
        [FormerlySerializedAs("velocitySmoothTime")]
        [Tooltip("Approximately how long it should take for the running animation to match the actual velocity. " +
                 "Increasing this value can help avoid jarring transitions between the idle and walking animation.")]
        [Min(0)] public float runVelocitySmoothTime = .15f;
        [Tooltip("The duration of time in seconds the jump bool variable in the animator is true after jumping.")]
        [Min(0)] public float recentJumpDuration = .1f;
        [Tooltip("The name of the jump bool parameter in the animator.")]
        [SerializeField] string jumpName = "Jump";
        [Tooltip("Invoked when a foot hits the ground during a walk or run animation.")]
        [SerializeField] UnityEvent footStepped = new();
        [Tooltip("Invoked when the feet hit the ground during a landing animation.")]
        [SerializeField] UnityEvent landed = new();
        
        Animator animator;
        int? jumpId;
        int? velocityYId;
        int? velocityXId;
        int? animationSpeedId;
        int? groundedId;
        int? freeFallDurationId;
        float currentFreeFallDuration;
        Vector3 runVelocity;
        Vector3 runVelocitySmoothing;
        bool wasGrounded;
        
        /// <summary>
        /// Invoked when a foot hits the ground during a walk or run animation.
        /// </summary>
        public UnityEvent FootStepped => footStepped;
        /// <summary>
        /// Invoked when the feet hit the ground during a landing animation.
        /// </summary>
        public UnityEvent Landed => landed;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            characterMotor.Value ??= transform.root.GetComponentInChildren<ICharacterMotor>();
            jump.Value ??= transform.root.GetComponentInChildren<IJump>();
            // ClampAnimationCurve(runSpeedToValue, default, Vector2.one);
            #if UNITY_EDITOR
            for (int i = 0; i < runSpeedToValue.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(runSpeedToValue, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(runSpeedToValue, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            #endif
        }

        void Awake()
        {
            animator = GetComponent<Animator>();
            HashSet<string> parameterNames = new(animator.parameters.Select(p => p.name));
            jumpId = parameterNames.Contains(jumpName) ? Animator.StringToHash(jumpName) : null;
            velocityYId = parameterNames.Contains(velocityYName) ? Animator.StringToHash(velocityYName) : null;
            velocityXId = parameterNames.Contains(velocityXName) ? Animator.StringToHash(velocityXName) : null;
            animationSpeedId = parameterNames.Contains(animationSpeedName) ? Animator.StringToHash(animationSpeedName) : null;
            groundedId = parameterNames.Contains(groundedName) ? Animator.StringToHash(groundedName) : null;
            freeFallDurationId = parameterNames.Contains(freeFallDurationName) ? Animator.StringToHash(freeFallDurationName) : null;
        }

        void OnEnable()
        {
            currentFreeFallDuration = 0;
            if (!TryValidateRequiredField(this, characterMotor.Value)
                | !TryValidateRequiredField(this, jump.Value))
            {
                enabled = false;
            }
        }

        void LateUpdate()
        {
            if (groundedId.HasValue) animator.SetBool(groundedId.Value, characterMotor.Value.IsGrounded);
            if (characterMotor.Value.IsGrounded && wasGrounded)
            {
                currentFreeFallDuration = 0;
            }
            else
            {
                currentFreeFallDuration += Time.deltaTime;
            }
            if (freeFallDurationId.HasValue) animator.SetFloat(freeFallDurationId.Value, currentFreeFallDuration);
            
            Vector3 runVelocityGoal = characterMotor.Value.Rigidbody.linearVelocity - characterMotor.Value.Ground.PointSecantVelocity;
            runVelocity = Vector3.SmoothDamp(runVelocity, runVelocityGoal, ref runVelocitySmoothing, runVelocitySmoothTime);
            Offset2 runValue;
            if (velocityXId.HasValue)
            {
                runValue = new Offset2(
                    Vector3.Dot(transform.right, runVelocity),
                    Vector3.Dot(transform.forward, runVelocity));
            }
            else
            {
                runValue = new Offset2(0, runVelocity.WithY(0).magnitude);
            }
            runValue.Magnitude = runSpeedToValue.Evaluate(runValue.Magnitude);
            if (velocityXId.HasValue) animator.SetFloat(velocityXId.Value, runValue.X);
            if (velocityYId.HasValue) animator.SetFloat(velocityYId.Value, runValue.Y);
            if (animationSpeedId.HasValue) animator.SetFloat(animationSpeedId.Value, Mathf.Max(runValue.Magnitude, 1));
            
            bool hasRecentlyJumped = Time.timeAsDouble < jump.Value.LastJumpTime + recentJumpDuration;
            if (jumpId.HasValue) animator.SetBool(jumpId.Value, hasRecentlyJumped);

            wasGrounded = characterMotor.Value.IsGrounded;
        }
        
        /// <summary>
        /// Invoke the <see cref="FootStepped"/> UnityEvent. Call this from an Animation Event in a walk or run
        /// animation when each foot hits the ground.
        /// </summary>
        public void OnFootstep()
        {
            footStepped?.Invoke();
        }
        
        [Obsolete]
        public void InvokeFootSteppedEvent()
        {
            footStepped?.Invoke();
        }

        /// <summary>
        /// Invoke the <see cref="Landed"/> UnityEvent. Call this from an Animation Event in a landing animation
        /// when the feet hit the ground.
        /// </summary>
        public void OnLand()
        {
            landed?.Invoke();
        }
        
        [Obsolete]
        public void InvokeLandedEvent()
        {
            landed?.Invoke();
        }

        static AnimationCurve GetDefaultUnityRobotCurve()
        {
            return new AnimationCurve(
                new Keyframe(0, 0),
                new Keyframe(.15f, .09f),
                new Keyframe(.25f, .125f),
                new Keyframe(.5f, .19f),
                new Keyframe(1, .25f),
                new Keyframe(1.5f, .28f),
                new Keyframe(2, .3f),
                new Keyframe(2.5f, .43f),
                new Keyframe(3, .55f),
                new Keyframe(4, .76f),
                new Keyframe(5, .92f),
                new Keyframe(6, 1.05f),
                new Keyframe(7, 1.25f));
        }
    }
}