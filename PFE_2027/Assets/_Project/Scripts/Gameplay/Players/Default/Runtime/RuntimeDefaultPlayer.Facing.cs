using Helteix.ChanneledProperties.Priorities;
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

        private Priority<Vector3> facingDirection;
        
        private void InitializeFacing()
        {
            facingDirection = new Priority<Vector3>();
        }
        
        // Called every FixedUpdate from OnCharacterMoving, before the constraints
        private void CaptureMoveIntent(ICharacterMotor motor)
        {
            moveIntent = motor.MoveInput;
        }

        private void UpdateFacing()
        {
            var facing = facingDirection.Value;
            mesh.rotation.SetLookRotation(facing);
        }
    }
}
