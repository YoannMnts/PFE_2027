using PFE.Core.Scripts.Curves;
using PFE.Core.Scripts.Players;
using TraversalPro;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Dash of PlayerData.DashDistance meters, shaped by PlayerData.DashProgressOverTime.
    // The CharacterMotor keeps moving the rigidbody (walls, slopes and steps are handled), the dash only
    // replaces its velocity goal while it lasts. MoveInput is never touched: it is the player's intent, and
    // CharacterRun doesn't clear it while no input is held (a dash direction left in it would keep running).
    public partial class RuntimeDefaultPlayer
    {
        // Acceleration given to the motor during the dash: high enough to reach the dash speed in one step
        private const float DASH_ACCELERATION = 1000f;

        public bool IsDashing { get; private set; }

        private AnimationCurve dashCurve;
        private Vector3 dashDirection;
        private float dashDistance;
        private float dashDuration;
        private float dashElapsed;
        private float dashReadyTime;
        private bool isEndingDash;      // The step right after the dash hands the velocity back to the input

        // Called by the input (CharacterControllerExtension)
        public void Dash()
        {
            PlayerData data = Player?.Data;
            if (data == null || IsDashing || Time.time < dashReadyTime)
                return;

            // The CharacterMotor only drives the velocity on the ground
            if (characterMotor == null || !characterMotor.IsGrounded)
                return;

            float duration = data.DashProgressOverTime.GetProgressDuration();
            if (duration <= 0f || data.DashDistance <= 0f)
                return;

            // The dash goes where the player is aiming, otherwise straight ahead
            Vector3 direction = mesh.forward;
            direction.y = 0f;
            dashDirection = direction.normalized;

            // A dash cancels the current attack
            hitboxRunner?.End();

            dashCurve = data.DashProgressOverTime;
            dashDistance = data.DashDistance;
            dashDuration = duration;
            dashElapsed = 0f;
            isEndingDash = false;
            IsDashing = true;
        }

        // Called every FixedUpdate from OnCharacterMoving, before the other constraints
        private void ApplyDash(ICharacterMotor motor)
        {
            if (isEndingDash)
            {
                // CharacterRun already wrote the goal from the current input (run speed if a direction is held,
                // zero otherwise): reach it at once, so the dash neither stops the player dead nor lets it slide
                isEndingDash = false;
                motor.AccelerationGoal = DASH_ACCELERATION;
                return;
            }

            if (!IsDashing)
                return;

            float step = Time.fixedDeltaTime;
            float previousTime = dashElapsed;
            dashElapsed = Mathf.Min(dashElapsed + step, dashDuration);

            // Distance to cover during this step, turned into the velocity the motor must reach:
            // the total always ends exactly on DashDistance
            float speed = dashCurve.EvaluateProgressDelta(previousTime, dashElapsed) * dashDistance / step;

            motor.LocalVelocityGoal = dashDirection * speed;
            motor.AccelerationGoal = DASH_ACCELERATION;
            motor.MaxLocalSpeed = Mathf.Max(motor.MaxLocalSpeed, speed);

            if (dashElapsed < dashDuration)
                return;

            IsDashing = false;
            isEndingDash = true;
            dashCurve = null;
            dashReadyTime = Time.time + Player.Data.DashCooldown;
        }
    }
}
