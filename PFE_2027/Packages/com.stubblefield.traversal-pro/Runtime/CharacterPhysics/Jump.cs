using UnityEngine;
using UnityEngine.InputSystem;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Applies instantaneous upward acceleration to this character to simulate jumping.
    /// A Rigidbody component and an <see cref="IGrounding"/> are required to be attached to this GameObject.
    /// </summary>
    [RequireComponent(typeof(ICharacterMotor))]
    [AddComponentMenu("Traversal Pro/Character Physics/Jump")]
    public class Jump : MonoBehaviour, IJump
    {
        [Tooltip("Approximately how high in meters the character will jump.")]
        [Min(0)] public float height = 1;
        [Tooltip("How long in seconds after jumping before the character can jump again.")] 
        [Min(0)] public float cooldownDuration = .5f;
        [Tooltip("How long in seconds the character can perform a jump before becoming grounded or after becoming ungrounded.")]
        [Min(0)] public float graceDuration = .1f;
        [Tooltip("How long in seconds to wait after a successful jump input before the jump force is applied+.")]
        [Min(0)] public float delay = .1f;
        [Tooltip("The max force in newtons to apply to the ground when jumping.")]
        [Min(0)] public float maxGroundForce = 100000;
        [Tooltip("Optional. The IFeeFall component attached to this character if any.")]
        public InterfaceRef<IFreeFall> freeFall;
        InterfaceRef<ICharacterMotor> characterMotor;
        double jumpRequestTime;
        double jumpForceTime = double.MaxValue;
        double lastGroundedTime;
        const float recentJumpDuration = .2f;
        
        public double LastJumpTime { get; private set; }

        public Rigidbody Rigidbody => characterMotor.Value.Rigidbody;

        void OnValidate()
        {
            freeFall.Value ??= GetComponent<IFreeFall>();
        }

        void Awake()
        {
            if (!TryValidateRequiredComponent(this, ref characterMotor)) enabled = false;
        }

        void OnEnable()
        {
            LastJumpTime = float.MinValue;
            characterMotor.Value.Moving += CheckForJump;
        }

        void OnDisable()
        {
            characterMotor.Value.Moving -= CheckForJump;
        }

        void CheckForJump(ICharacterMotor _)
        {
            double time = Time.timeAsDouble;
            if (characterMotor.Value.IsGrounded) lastGroundedTime = time;
            if (ShouldJump())
            {
                LastJumpTime = time;
                jumpForceTime = time + delay;
            }
            if (time >= jumpForceTime)
            {
                jumpForceTime = double.MaxValue;
                PerformJump();
            }
            if (time >= LastJumpTime + delay 
                && time <= LastJumpTime + delay + recentJumpDuration)
            {
                characterMotor.Value.IsGrounded = false;
            }
        }

        bool ShouldJump()
        {
            if (jumpRequestTime == 0) return false; // uninitialized
            double time = Time.timeAsDouble;
            if (time < jumpRequestTime) return false; // future
            if (time > jumpRequestTime + graceDuration) return false; // too old
            if (characterMotor.Value.IsOnSteepSlope) return false;
            if (jumpRequestTime < LastJumpTime + cooldownDuration) return false; // needs cooldown
            if (characterMotor.Value.IsGrounded) return true;
            if (time < lastGroundedTime + graceDuration) return true; // within grace time
            return false;
        }

        /// <summary>
        /// Immediately apply upward acceleration to this character and downward force on the ground to simulate a jump.
        /// </summary>
        public void PerformJump()
        {
            if (Time.deltaTime <= 0) return;
            Vector3 velocity = characterMotor.Value.Rigidbody.linearVelocity;
            float gravityValue = GetGravity(characterMotor.Value.Rigidbody, freeFall.Value).y;
            float jumpVelocity = JumpSpeed(gravityValue, height);
            float priorVelocityY = velocity.y;
            velocity.y = jumpVelocity + characterMotor.Value.Ground.PointVelocity.y;
            characterMotor.Value.Rigidbody.SetVelocity(velocity);
            if (characterMotor.Value.Ground.Rigidbody)
            {
                float mass = characterMotor.Value.Rigidbody.mass;
                float force = (velocity.y - priorVelocityY) / Time.deltaTime * mass;
                force = Mathf.Clamp(force, 0, maxGroundForce);
                float gravityForce = gravityValue * mass;
                Vector3 groundForce = new Vector3(0, gravityForce - force, 0);
                characterMotor.Value.Ground.Rigidbody.AddForceAtPosition(groundForce, characterMotor.Value.Ground.Point, ForceMode.Force);
            }
        }
        
        public void RequestJump()
        {
            jumpRequestTime = Time.timeAsDouble;
        }
        
        /// <summary>
        /// Request a jump using Unity's Input System. Various jump rules will be checked to determine
        /// if a jump is allowed. The jump may occur on a later frame.
        /// </summary>
        public void RequestJump(InputAction.CallbackContext context)
        {
            if (!isActiveAndEnabled) return;
            if (context.phase == InputActionPhase.Started)
            {
                RequestJump();
            }
        }
    }
}