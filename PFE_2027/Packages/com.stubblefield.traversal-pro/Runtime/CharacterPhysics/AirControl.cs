using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Applies forces to an attached Rigidbody component while in free fall according to user input.
    /// A Rigidbody component attached to the same GameObject is required.
    /// An <see cref="IGrounding"/> component is required to be attached to this GameObject.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Character Physics/Air Control")]
    [RequireComponent(typeof(ICharacterMotor))]
    public class AirControl : MonoBehaviour
    {
        [Tooltip("How long air control lasts after beginning free fall.")]
        [Min(0)] public float duration = float.PositiveInfinity;
        [Tooltip("The horizontal air control speed goal when in free fall.")]
        [Min(0)] public float speed = 5;
        [Tooltip("The maximum horizontal speed under which the character still has air control.")]
        [Min(0)] public float maxControllableSpeed = 8;
        [Tooltip("The horizontal acceleration.")]
        [Min(0)] public float acceleration = 10;
        InterfaceRef<ICharacterMotor> characterMotor;
        
        float currentDuration;
        Capsule capsule;
        
        /// <summary>
        /// The world space velocity goal for this character while in free fall. The Y value is ignored. If
        /// <see cref="LocalMoveInput"/> has a value, then it will override <see cref="VelocityGoal"/>.
        /// </summary>
        public Vector3 VelocityGoal { get; set; }

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            maxControllableSpeed = Mathf.Max(maxControllableSpeed, speed);
        }

        void Awake()
        {
            TryValidateRequiredComponent(this, ref characterMotor);
        }

        void FixedUpdate()
        {
            if (characterMotor.Value.IsGrounded)
            {
                currentDuration = 0;
                VelocityGoal = default;
                return;
            }
            
            currentDuration += Time.deltaTime;
            if (currentDuration > duration) return;
            
            if (characterMotor.Value.MoveInput.sqrMagnitude > 0)
            {
                VelocityGoal = characterMotor.Value.MoveInput * speed;
            }
            Offset3 targetHorizontalVelocity = new(VelocityGoal.WithY(0));
            if (targetHorizontalVelocity.Magnitude < speed * .1f) return;
            
            Vector3 priorVelocity = characterMotor.Value.Rigidbody.linearVelocity;
            Vector3 velocity = priorVelocity;
            Offset3 currentHorizontalVelocity = new(velocity.WithY(0));
            if (currentHorizontalVelocity.Magnitude > maxControllableSpeed) return;
            
            targetHorizontalVelocity.Magnitude = Mathf.Max(targetHorizontalVelocity.Magnitude, currentHorizontalVelocity.Magnitude);
            Vector3 newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity.Value, 
                targetHorizontalVelocity.Value, 
                acceleration * Time.deltaTime);
            velocity.x = newHorizontalVelocity.x;
            velocity.z = newHorizontalVelocity.z;
            characterMotor.Value.Rigidbody.AddForce(velocity - priorVelocity, ForceMode.VelocityChange);
        }
    }
}