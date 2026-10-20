using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using PFE.Core.Scripts.Enemy.Attacks;
using PrimeTween;
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
        private const float FACING_INPUT_DEAD_ZONE = 0.1f;
        private const float FACING_RETARGET_ANGLE = 2f;

        [SerializeField, BoxGroup("Facing")]
        private TweenSettings facingTweenSettings;

        private ChannelKey inputChannel;

        private Priority<Vector3> targetFacingDirection;

        private Tween facingTween;
        private Quaternion facingGoal;
        
        private void InitializeFacing()
        {
            targetFacingDirection = new Priority<Vector3>();
            inputChannel = ChannelKey.GetUniqueChannelKey();
            targetFacingDirection.AddPriority(inputChannel, PriorityTags.Smallest, Vector3.zero);

            // Current orientation as the first goal: a direction already faced doesn't start a turn
            facingGoal = mesh.rotation;
            
            targetFacingDirection.OnValueChanged += TargetFacingDirectionValueChanged;
        }

        private void DisposeFacing()
        {
            targetFacingDirection.OnValueChanged -= TargetFacingDirectionValueChanged;
            targetFacingDirection.RemovePriority(inputChannel);
            targetFacingDirection = null;

            if (facingTween.isAlive)
                facingTween.Stop();
        }
        
        // Called every FixedUpdate from OnCharacterMoving, before the constraints
        private void CaptureMoveIntent(ICharacterMotor motor)
        {
            targetFacingDirection.Write(inputChannel, motor.MoveInput);
        }

        private void TargetFacingDirectionValueChanged(Vector3 direction)
        {
            direction.y = 0f;

            if (direction.sqrMagnitude < FACING_INPUT_DEAD_ZONE * FACING_INPUT_DEAD_ZONE)
                return;

            Quaternion goal = Quaternion.LookRotation(direction);
            if (Quaternion.Angle(goal, facingGoal) < FACING_RETARGET_ANGLE)
                return;

            facingGoal = goal;

            if (facingTween.isAlive)
                facingTween.Stop();

            facingTween = Tween.Rotation(mesh, new TweenSettings<Quaternion>(goal, facingTweenSettings));
        }
    }
}
