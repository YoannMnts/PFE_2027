using PFE.Gameplay.Scripts.Animations;
using Sirenix.OdinInspector;
using TraversalPro;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Locomotion parameters of the controller, sent to the ActionAnimator graph. Same logic as Traversal Pro's
    // CharacterAnimator (which must be disabled on the prefab: its Animator.SetX calls no longer reach the controller).
    public partial class RuntimeDefaultPlayer
    {
        private static readonly int VELOCITY_Y_ID = Animator.StringToHash("VelocityY");
        private static readonly int ANIMATION_SPEED_ID = Animator.StringToHash("AnimationSpeed");
        private static readonly int GROUNDED_ID = Animator.StringToHash("Grounded");
        private static readonly int FREE_FALL_DURATION_ID = Animator.StringToHash("FreeFallDuration");
        private static readonly int JUMP_ID = Animator.StringToHash("Jump");

        // Time for the run animation to match the actual velocity
        [SerializeField, BoxGroup("Animation"), Min(0f)]
        private float runVelocitySmoothTime = 0.15f;

        // How long (in seconds) the Jump parameter stays true after jumping
        [SerializeField, BoxGroup("Animation"), Min(0f)]
        private float recentJumpDuration = 0.1f;

        // Converts the movement speed (m/s) into the blend tree value (Traversal Pro's curve for the UnityRobot clips)
        [SerializeField, BoxGroup("Animation")]
        private AnimationCurve runSpeedToValue = new(
            new Keyframe(0f, 0f), new Keyframe(0.15f, 0.09f), new Keyframe(0.25f, 0.125f), new Keyframe(0.5f, 0.19f),
            new Keyframe(1f, 0.25f), new Keyframe(1.5f, 0.28f), new Keyframe(2f, 0.3f), new Keyframe(2.5f, 0.43f),
            new Keyframe(3f, 0.55f), new Keyframe(4f, 0.76f), new Keyframe(5f, 0.92f), new Keyframe(6f, 1.05f),
            new Keyframe(7f, 1.25f));

        private ActionAnimator actionAnimator;
        private IJump jump;

        private Vector3 runVelocity;
        private Vector3 runVelocitySmoothing;
        private float freeFallDuration;
        private bool wasGrounded;

        private void InitializeAnimation()
        {
            if (!animator.TryGetComponent(out actionAnimator))
                Debug.LogError("[RuntimeDefaultPlayer] No ActionAnimator next to the Animator.", this);

            jump = transform.root.GetComponentInChildren<IJump>();
        }

        // Called in LateUpdate, like Traversal Pro's CharacterAnimator
        private void UpdateLocomotionAnimation()
        {
            if (actionAnimator == null || characterMotor == null)
                return;

            bool isGrounded = characterMotor.IsGrounded;
            actionAnimator.SetBool(GROUNDED_ID, isGrounded);

            freeFallDuration = isGrounded && wasGrounded ? 0f : freeFallDuration + Time.deltaTime;
            actionAnimator.SetFloat(FREE_FALL_DURATION_ID, freeFallDuration);

            Vector3 runVelocityGoal = characterMotor.Rigidbody.linearVelocity - characterMotor.Ground.PointSecantVelocity;
            runVelocity = Vector3.SmoothDamp(runVelocity, runVelocityGoal, ref runVelocitySmoothing, runVelocitySmoothTime);

            Vector3 horizontal = runVelocity;
            horizontal.y = 0f;
            float runValue = runSpeedToValue.Evaluate(horizontal.magnitude);
            actionAnimator.SetFloat(VELOCITY_Y_ID, runValue);
            actionAnimator.SetFloat(ANIMATION_SPEED_ID, Mathf.Max(runValue, 1f));

            bool hasRecentlyJumped = jump != null && Time.timeAsDouble < jump.LastJumpTime + recentJumpDuration;
            actionAnimator.SetBool(JUMP_ID, hasRecentlyJumped);

            wasGrounded = isGrounded;
        }
    }
}
