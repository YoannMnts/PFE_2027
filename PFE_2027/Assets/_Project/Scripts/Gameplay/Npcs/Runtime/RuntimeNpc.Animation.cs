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

        private float animatedSpeed;
        private float animatedSpeedVelocity;
        
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
        
        private void UpdateAnimator()
        {
            if (Animator == null || NavMeshAgent == null)
                return;

            Vector3 velocity = NavMeshAgent.velocity;
            velocity.y = 0f;

            animatedSpeed = Mathf.SmoothDamp(animatedSpeed, velocity.magnitude, ref animatedSpeedVelocity, ANIMATION_SPEED_SMOOTH_TIME);

            float runValue = runSpeedToValue.Evaluate(animatedSpeed);
            Animator.SetFloat(VELOCITY_Y_ID, runValue);
            Animator.SetFloat(ANIMATION_SPEED_ID, Mathf.Max(runValue, 1f));
        }
    }
}