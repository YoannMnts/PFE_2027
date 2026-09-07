using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Rotates the transform on this GameObject to simulate a character leaning into a turning acceleration while running.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Animation/Acceleration Roll Animation")]
    public class AccelerationRollAnimation : MonoBehaviour
    {
        [Tooltip("The ICharacterMotor component to observe for acceleration values.")]
        public InterfaceRef<ICharacterMotor> characterMotor;
        [Tooltip("The max angle the character will lean while walking or running.")]
        public float leanAngle = 10;
        Vector2 smoothedAcceleration;
        Vector2 smoothedAccelerationVelocity;
        Vector3 velocity;
        const float minLeanSmoothTime = .05f;
        const float maxLeanSmoothTime = .3f;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            characterMotor.Value ??= transform.root.GetComponentInChildren<ICharacterMotor>();
            if (enabled && HasForbiddenComponent<VelocityYawAnimation>(this)) enabled = false;
            if (enabled && HasForbiddenComponent<SmoothYawConstraint>(this)) enabled = false;
        }

        void OnEnable()
        {
            if (TryValidateRequiredField(this, characterMotor.Value))
            {
                velocity = characterMotor.Value.Rigidbody.linearVelocity;
            }
            else
            {
                enabled = false;
            }
        }

        void LateUpdate()
        {
            if (Time.deltaTime <= 0) return;
            Vector3 priorVelocity = velocity;
            velocity = characterMotor.Value.Rigidbody.linearVelocity;
            Vector2 rbAccelerationXZ = ((velocity - priorVelocity) / Time.deltaTime).XZ();
            float localSpeed = (velocity - characterMotor.Value.Ground.PointSecantVelocity).magnitude;
            bool hasSignificantVelocity = localSpeed > characterMotor.Value.MaxLocalSpeed * .25f;
            if (!characterMotor.Value.IsGrounded
                || characterMotor.Value.IsOnSteepSlope || !hasSignificantVelocity)
            {
                rbAccelerationXZ = default;
            }
            float smoothTimeLerpT = Mathf.Clamp01(localSpeed / characterMotor.Value.MaxLocalSpeed);
            float smoothTime = Mathf.Lerp(minLeanSmoothTime, maxLeanSmoothTime, smoothTimeLerpT);
            smoothedAcceleration = Vector2.SmoothDamp(smoothedAcceleration, rbAccelerationXZ, ref smoothedAccelerationVelocity, smoothTime);
            float leanValue = -Vector2.Dot(smoothedAcceleration, transform.right.XZ());
            leanValue /= characterMotor.Value.MaxAcceleration;
            leanValue = Mathf.Clamp(leanValue, -1, 1);
            transform.localRotation = Quaternion.Euler(0, 0, leanValue * leanAngle);
        }
    }
}