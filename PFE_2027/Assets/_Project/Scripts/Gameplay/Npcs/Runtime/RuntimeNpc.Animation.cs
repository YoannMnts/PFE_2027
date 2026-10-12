using PFE.Gameplay.Scripts.Animations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    public abstract partial class RuntimeNpc
    {
        private const float ANIMATION_SPEED_SMOOTH_TIME = 0.15f;

        private static readonly int VELOCITY_Y_ID = Animator.StringToHash("VelocityY");
        private static readonly int ANIMATION_SPEED_ID = Animator.StringToHash("AnimationSpeed");

        [field : SerializeField, BoxGroup("References")]
        public Animator Animator { get; private set; }
        
        [SerializeField, BoxGroup("GameFeel")] 
        private AnimationCurve runSpeedToValue;

        // On the Animator's GameObject: plays the locomotion controller and the action clips
        public ActionAnimator ActionAnimator { get; private set; }

        private float animatedSpeed;
        private float animatedSpeedVelocity;

        private void InitializeAnimation()
        {
            if (Animator == null)
                return;

            if (Animator.TryGetComponent(out ActionAnimator actionAnimator))
                ActionAnimator = actionAnimator;
            else
                Debug.LogError($"[RuntimeNpc] '{name}': no ActionAnimator next to its Animator.", this);
        }
        
        private void FaceMovement()
        {
            if (Mesh == null || NavMeshAgent == null)
                return;

            Vector3 velocity = NavMeshAgent.velocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            Mesh.rotation = Quaternion.RotateTowards(Mesh.rotation, targetRotation, NavMeshAgent.angularSpeed * Time.deltaTime);
        }
        
        // Feeds the locomotion blend tree (through the ActionAnimator graph) with the agent's horizontal speed
        private void UpdateAnimator()
        {
            if (ActionAnimator == null || NavMeshAgent == null)
                return;

            Vector3 velocity = NavMeshAgent.velocity;
            velocity.y = 0f;

            animatedSpeed = Mathf.SmoothDamp(animatedSpeed, velocity.magnitude, ref animatedSpeedVelocity, ANIMATION_SPEED_SMOOTH_TIME);

            float runValue = runSpeedToValue.Evaluate(animatedSpeed);
            ActionAnimator.SetFloat(VELOCITY_Y_ID, runValue);
            ActionAnimator.SetFloat(ANIMATION_SPEED_ID, Mathf.Max(runValue, 1f));
        }
    }
}
