using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Chooses the run speed for the character and sets the <see cref="CharacterMotor.LocalVelocityGoal"/>
    /// and <see cref="CharacterMotor.AccelerationGoal"/> values on the required attached <see cref="CharacterMotor"/>
    /// component.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Character Physics/Character Run")]
    [RequireComponent(typeof(ICharacterMotor))]
    [DefaultExecutionOrder(-2000)]
    public class CharacterRun : MonoBehaviour
    {
        [Tooltip("The character's View transform.")]
        public Transform view;
        [Tooltip("The speed this character moves at while running.")]
        [Min(0)] public float runSpeed = 5;
        [Tooltip("The speed this character moves at while sprinting.")]
        [Min(0)] public float sprintSpeed = 7;
        [Tooltip("The acceleration of this character when the velocity goal is greater than the current velocity.")]
        [Min(0)] public float acceleration = 25;
        [Tooltip("The deceleration of this character when the velocity goal is less than the current velocity or when " +
                 "the velocity goal is roughly in the opposite direction of the current velocity.")]
        [Min(0)] public float deceleration = 35;
        InterfaceRef<ICharacterMotor> characterMotor;
        Vector3? localMoveInput;
        const float sprintSpeedPercentThreshold = .5f;
        
        /// <summary>
        /// Is the character currently sprinting? Assign a value to make the character start or stop sprinting.
        /// </summary>
        public bool IsSprinting { get; set; }

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            if (!view)
            {
                ViewControl viewControl = GetComponentInChildren<ViewControl>();
                if (viewControl) view = viewControl.transform;
            }
        }

        void OnEnable()
        {
            if (!TryValidateRequiredField(this, view)
                || !TryValidateRequiredComponent(this, ref characterMotor))
            {
                enabled = false;
            }
        }

        void FixedUpdate()
        {
            if (localMoveInput.HasValue)
            {
                characterMotor.Value.MoveInput = RotateHorizontally(localMoveInput.Value, -Yaw(view.rotation));
            }

            float localSpeed = (characterMotor.Value.Rigidbody.linearVelocity - characterMotor.Value.Ground.PointSecantVelocity).magnitude;
            float inputValue = characterMotor.Value.MoveInput.magnitude;
            IsSprinting &= localSpeed > runSpeed * sprintSpeedPercentThreshold
                           && inputValue > sprintSpeedPercentThreshold;
            characterMotor.Value.MaxLocalSpeed = Mathf.Max(runSpeed, sprintSpeed);
            characterMotor.Value.MaxAcceleration = Mathf.Max(acceleration, deceleration);
            characterMotor.Value.LocalVelocityGoal = characterMotor.Value.MoveInput * (IsSprinting ? sprintSpeed : runSpeed);
            Vector3 neededVelocityDelta = characterMotor.Value.LocalVelocityGoal - characterMotor.Value.LocalVelocity;
            float t = (Vector3.Dot(neededVelocityDelta.normalized, characterMotor.Value.LocalVelocity.normalized) + 1) * .5f;
            characterMotor.Value.AccelerationGoal = Mathf.Lerp(deceleration, acceleration, t);
        }

        /// <summary>
        /// Make the character start sprinting. This is useful for toggle-to-sprint controls.
        /// </summary>
        /// <param name="context">This will be read as a button press. If the button is pressed, then
        /// <see cref="IsSprinting"/> will be set to true. If not, then this method does nothing.</param>
        public void StartSprinting(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled) return;
            if (context.ReadValueAsButton()) IsSprinting = true;
        }

        /// <summary>
        /// Make the character start or stop sprinting. This is useful for hold-to-sprint controls.
        /// </summary>
        /// <param name="context">This will be read as a button press. <see cref="IsSprinting"/> will be assigned
        /// the value of the button, whether true or false.</param>
        public void SetIsSprinting(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled) return;
            IsSprinting = context.ReadValueAsButton();
        }

        /// <summary>
        /// Set the character's local move input using Unity InputSystem. This value will be rotated every
        /// frame by the y axis of the <see cref="view"/> rotation to determine the world direction the
        /// character should move in.
        /// </summary>
        /// <param name="context"></param>
        public void MoveInput(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled) return;
            if (context.phase == InputActionPhase.Canceled)
            {
                localMoveInput = null;
                characterMotor.Value.MoveInput = default;
                return;
            }
            Vector2 value;
            try
            {
                value = context.ReadValue<Vector2>();
            }
            catch (Exception e)
            {
                LogError($"Unable to read value of type Vector2 from {context.action.name}. Error message: {e.Message}");
                return;
            }
            localMoveInput = new Vector3(value.x, 0, value.y);
        }
    }
}