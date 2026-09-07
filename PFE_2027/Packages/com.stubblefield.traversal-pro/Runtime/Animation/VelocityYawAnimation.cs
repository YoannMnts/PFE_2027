using System;
using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Rotates a character mesh to align with the appropriate forward direction.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Animation/Velocity Yaw Animation")]
    public class VelocityYawAnimation : MonoBehaviour
    {
        [Tooltip("The ICharacterMotor component for the animated character.")]
        public InterfaceRef<ICharacterMotor> characterMotor;
        [Tooltip("The View Control component to read values from for this character.")]
        public Transform view;
        [Tooltip("The minimum factor of the character's max move speed that the character must be moving " +
                 "at for the yaw animation to begin happening.")]
        [Range(0, 1)] public float minSpeedFactor = .05f;
        public AnimationCurve speedToSmoothTime = new(new Keyframe(0, .2f, -.05f, -.05f), new Keyframe(6, .1f));
        public bool duringFreeFall;
        [Obsolete, HideInInspector] public float smoothTime = .1f;
        float yawGoal;
        float yaw;
        float yawVelocity;
        bool isRotating;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            characterMotor.Value ??= transform.root.GetComponentInChildren<ICharacterMotor>();
            if (enabled && HasForbiddenComponent<AccelerationRollAnimation>(this)) enabled = false;
            if (!view)
            {
                ViewControl viewControl = transform.root.GetComponentInChildren<ViewControl>();
                if (viewControl) view = viewControl.transform;
            }
            ClampAnimationCurve(speedToSmoothTime, default, new Vector2(float.MaxValue, float.MaxValue));
        }

        void OnEnable()
        {
            if (!TryValidateRequiredField(this, characterMotor.Value)) enabled = false;
            if (TryValidateRequiredField(this, view))
            {
                yawGoal = Yaw(view.rotation);
                yaw = yawGoal;
            }
            else
            {
                enabled = false;
            }
        }

        void LateUpdate()
        {
            if (!duringFreeFall && !characterMotor.Value.IsGrounded) return;
            
            if (characterMotor.Value.IsGrounded
                && characterMotor.Value.Ground.Rigidbody)
            {
                float groundYawDelta = characterMotor.Value.Ground.AngularVelocity.y * Mathf.Rad2Deg * Time.deltaTime;
                yaw += groundYawDelta;
                yawGoal += groundYawDelta;
            }

            Vector2 localVelocity = characterMotor.Value.LocalVelocity.XZ();
            Vector2 localVelocityGoal = characterMotor.Value.LocalVelocityGoal.XZ();
            bool wasRotating = isRotating;
            float minSpeed = characterMotor.Value.MaxLocalSpeed * minSpeedFactor;
            if (localVelocity.magnitude > minSpeed 
                || localVelocityGoal.magnitude > minSpeed)
            {
                yawGoal = Yaw(localVelocity * 1.5f + localVelocityGoal);
                isRotating = true;
            }
            else if (wasRotating)
            {
                isRotating = false;
                yawGoal = Mathf.Lerp(yaw, yawGoal, .5f);
            }
            RecenterDegrees(ref yaw, ref yawGoal);
            float currentSmoothTime = speedToSmoothTime.Evaluate(localVelocity.magnitude);
            yaw = Mathf.SmoothDamp(yaw, yawGoal, ref yawVelocity, currentSmoothTime);
            transform.rotation = Quaternion.Euler(0, yaw, 0);
        }
    }
}