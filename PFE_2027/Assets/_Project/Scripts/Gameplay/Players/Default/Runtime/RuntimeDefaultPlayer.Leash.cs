using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.Pilgrims;
using Sirenix.OdinInspector;
using TraversalPro;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        [SerializeField, BoxGroup("Pilgrim Leash"), Min(0f)]
        private float targetGroupWeight = .5f;
        
        // Pilgrim the player is leashed to, only set during the ProtectPilgrimPhase
        private PilgrimInstance pilgrim;

        // Leash rules come from the game settings: they belong to the "protect the pilgrim" mode, not to the character
        private static float LeashRadius => GameMetricsSettings.Current.LeashRadius;
        private static float LeashPullBackSpeed => GameMetricsSettings.Current.LeashPullBackSpeed;

        // Keeps the player inside a horizontal circle around the pilgrim. At the edge, the outward part
        // of the velocity goal is removed so the player slides along the circle instead of leaving it.
        private void ApplyPilgrimLeash(ICharacterMotor motor)
        {
            if (pilgrim == null || pilgrim.IsDead)
                return;

            float radius = LeashRadius;

            Vector3 fromCenter = motor.Rigidbody.position - pilgrim.CurrentPosition;
            fromCenter.y = 0f;

            float distance = fromCenter.magnitude;
            if (distance < radius)
                return;

            Vector3 outward = fromCenter / distance;
            Vector3 goal = motor.LocalVelocityGoal;

            float outwardSpeed = Vector3.Dot(goal, outward);
            if (outwardSpeed > 0f)
                goal -= outward * outwardSpeed;

            // Already outside (the pilgrim walked away): pull the player back towards the circle
            if (distance > radius + 0.1f)
                goal -= outward * LeashPullBackSpeed;

            motor.LocalVelocityGoal = goal;
        }

    }
}