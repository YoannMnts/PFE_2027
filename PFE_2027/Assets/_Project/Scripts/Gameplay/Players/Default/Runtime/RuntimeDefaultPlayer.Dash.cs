using PFE.Core.Scripts.Players;
using TraversalPro;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Dash driven by PlayerData.DashDistanceInTime (distance travelled over time).
    // The CharacterMotor keeps moving the rigidbody (walls, slopes and steps are handled), the dash only
    // replaces its velocity goal while it lasts.
    public partial class RuntimeDefaultPlayer
    {
        // Acceleration given to the motor during the dash: high enough to reach the dash speed in one step
        private const float DASH_ACCELERATION = 1000f;

        public bool IsDashing { get; private set; }

        private Vector3 dashDirection;
        private float dashDuration;
        private float dashElapsed;
        private float dashReadyTime;

        // Called by the input (CharacterControllerExtension)
        public void Dash()
        {
            PlayerData data = Player?.Data;
            if (data == null || IsDashing || Time.time < dashReadyTime)
                return;

            // The CharacterMotor only drives the velocity on the ground
            if (characterMotor == null || !characterMotor.IsGrounded)
                return;

            AnimationCurve curve = data.DashDistanceInTime;
            if (curve == null || curve.length == 0)
                return;

            dashDuration = curve[curve.length - 1].time;
            if (dashDuration <= 0f)
                return;

            // The dash goes where the player is aiming, otherwise straight ahead
            SnapFacingToIntent();
            Vector3 direction = mesh.forward;
            direction.y = 0f;
            dashDirection = direction.normalized;

            // A dash cancels the current attack
            attackRunner?.End();

            dashElapsed = 0f;
            IsDashing = true;
        }

        // Called every FixedUpdate from OnCharacterMoving, before the other constraints
        private void ApplyDash(ICharacterMotor motor)
        {
            if (!IsDashing)
                return;

            AnimationCurve curve = Player.Data.DashDistanceInTime;
            float step = Time.fixedDeltaTime;

            float previousTime = dashElapsed;
            dashElapsed = Mathf.Min(dashElapsed + step, dashDuration);

            // Distance to cover during this step, turned into the velocity the motor must reach:
            // the total always ends exactly on the last key of the curve
            float distance = curve.Evaluate(dashElapsed) - curve.Evaluate(previousTime);
            float speed = distance / step;

            motor.MoveInput = dashDirection;
            motor.LocalVelocityGoal = dashDirection * speed;
            motor.AccelerationGoal = DASH_ACCELERATION;
            motor.MaxLocalSpeed = Mathf.Max(motor.MaxLocalSpeed, speed);

            if (dashElapsed < dashDuration)
                return;

            IsDashing = false;
            dashReadyTime = Time.time + Player.Data.DashCooldown;
        }
    }
}
