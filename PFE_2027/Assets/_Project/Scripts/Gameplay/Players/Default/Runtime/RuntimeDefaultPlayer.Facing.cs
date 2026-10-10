using PFE.Core.Scripts.Enemy.Attacks;
using Sirenix.OdinInspector;
using TraversalPro;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // The mesh faces where the player WANTS to go (move input), not where it actually goes:
    // at the leash edge the velocity is bent along the circle, the input is not.
    // Replaces Traversal Pro's VelocityYawAnimation, which must be disabled on the prefab.
    public partial class RuntimeDefaultPlayer
    {
        // Below this input length the facing is kept (stick dead zone)
        private const float FACING_INPUT_DEAD_ZONE = 0.1f;

        // Time for the mesh to reach its target yaw: higher = smoother, lower = snappier
        [SerializeField, BoxGroup("Facing"), Min(0f)]
        private float facingSmoothTime = 0.08f;

        // Move input captured before the movement constraints (the attack lock zeroes MoveInput)
        private Vector3 moveIntent;

        private float facingYaw;
        private float facingYawGoal;
        private float facingYawVelocity;

        private void InitializeFacing()
        {
            facingYaw = mesh.eulerAngles.y;
            facingYawGoal = facingYaw;
            facingYawVelocity = 0f;
        }

        // Called every FixedUpdate from OnCharacterMoving, before the constraints
        private void CaptureMoveIntent(ICharacterMotor motor)
        {
            moveIntent = motor.MoveInput;
        }

        private void UpdateFacing()
        {
            // The swing and the dash keep the direction chosen when they started
            bool isLocked = IsDashing || (attackRunner != null && attackRunner.Has(AttackFlags.MovementLock));
            if (!isLocked && TryGetIntentYaw(out float intentYaw))
                facingYawGoal = intentYaw;

            facingYaw = Mathf.SmoothDampAngle(facingYaw, facingYawGoal, ref facingYawVelocity, facingSmoothTime);
            mesh.rotation = Quaternion.Euler(0f, facingYaw, 0f);
        }

        // Turns the mesh at once towards the move input: the attack goes where the stick points
        private void SnapFacingToIntent()
        {
            if (!TryGetIntentYaw(out float intentYaw))
                return;

            facingYaw = intentYaw;
            facingYawGoal = intentYaw;
            facingYawVelocity = 0f;
            mesh.rotation = Quaternion.Euler(0f, facingYaw, 0f);
        }

        private bool TryGetIntentYaw(out float yaw)
        {
            Vector3 intent = moveIntent;
            intent.y = 0f;

            if (intent.sqrMagnitude < FACING_INPUT_DEAD_ZONE * FACING_INPUT_DEAD_ZONE)
            {
                yaw = 0f;
                return false;
            }

            yaw = Mathf.Atan2(intent.x, intent.z) * Mathf.Rad2Deg;
            return true;
        }
    }
}
