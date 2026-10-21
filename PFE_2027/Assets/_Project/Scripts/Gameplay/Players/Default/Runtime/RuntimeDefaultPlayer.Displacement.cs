using PFE.Core.Scripts.Curves;
using PFE.Core.Scripts.Enemy.Attacks;
using TraversalPro;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Displacement windows of the actions played by the HitboxRunner (dash, lunging attacks...). Started when the
    // window opens, then driven at the physics rate: the CharacterMotor keeps moving the rigidbody (walls, slopes and
    // steps are handled), the displacement only replaces its velocity goal while it lasts. MoveInput is never touched:
    // it is the player's intent, and CharacterRun doesn't clear it while no input is held.
    public partial class RuntimeDefaultPlayer
    {
        // Acceleration given to the motor during a displacement: high enough to reach its speed in one step
        private const float DISPLACEMENT_ACCELERATION = 1000f;

        public bool IsDisplaced { get; private set; }

        private HitboxData displacementSource;      // Action that opened it: replaced by another one = stopped
        private AnimationCurve displacementCurve;
        private Vector3 displacementDirection;
        private float displacementDistance;
        private float displacementDuration;         // Seconds, length of the window
        private float displacementCurveDuration;    // X range of the curve, stretched over the window
        private float displacementElapsed;
        private bool isEndingDisplacement;          // The step right after hands the velocity back to the input

        // HitboxContext.onWindowEnter: a Displacement window opens, the player moves where it faces
        private void OnWindowEnter(IWindowFlag flag, float duration)
        {
            if (flag is not Displacement displacement)
                return;

            float curveDuration = displacement.ProgressOverWindow.GetProgressDuration();
            if (curveDuration <= 0f || duration <= 0f || displacement.Distance <= 0f)
                return;

            Vector3 direction = mesh.forward;
            direction.y = 0f;

            displacementSource = hitboxRunner.Current;
            displacementCurve = displacement.ProgressOverWindow;
            displacementDirection = direction.normalized;
            displacementDistance = displacement.Distance;
            displacementDuration = duration;
            displacementCurveDuration = curveDuration;
            displacementElapsed = 0f;
            isEndingDisplacement = false;
            IsDisplaced = true;
        }

        // Called every FixedUpdate from OnCharacterMoving, after the movement lock (a displacement overrides it)
        private void ApplyDisplacement(ICharacterMotor motor)
        {
            if (isEndingDisplacement)
            {
                // CharacterRun already wrote the goal from the current input (run speed if a direction is held,
                // zero otherwise or under a movement lock): reach it at once, no dead stop nor slide
                isEndingDisplacement = false;
                motor.AccelerationGoal = DISPLACEMENT_ACCELERATION;
                return;
            }

            if (!IsDisplaced)
                return;

            // Another action took over (next attack, dash...): the displacement stops with the one that opened it
            if (hitboxRunner.Current != null && hitboxRunner.Current != displacementSource)
            {
                StopDisplacement();
                return;
            }

            float step = Time.fixedDeltaTime;
            float previousTime = displacementElapsed;
            displacementElapsed = Mathf.Min(displacementElapsed + step, displacementDuration);

            // The window is mapped onto the X range of the curve: the total always ends exactly on Distance
            float scale = displacementCurveDuration / displacementDuration;
            float speed = displacementCurve.EvaluateProgressDelta(previousTime * scale, displacementElapsed * scale)
                          * displacementDistance / step;

            motor.LocalVelocityGoal = displacementDirection * speed;
            motor.AccelerationGoal = DISPLACEMENT_ACCELERATION;
            motor.MaxLocalSpeed = Mathf.Max(motor.MaxLocalSpeed, speed);

            if (displacementElapsed >= displacementDuration)
                StopDisplacement();
        }

        private void StopDisplacement()
        {
            IsDisplaced = false;
            isEndingDisplacement = true;
            displacementSource = null;
            displacementCurve = null;
        }
    }
}
