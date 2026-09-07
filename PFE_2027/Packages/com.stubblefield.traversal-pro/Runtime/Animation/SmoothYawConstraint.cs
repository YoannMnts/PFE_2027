using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Rotates this transform along the y axis to match the forward direction of the target transform.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Animation/Smooth Yaw Constraint")]
    public class SmoothYawConstraint : MonoBehaviour
    {
        [Tooltip("The Transform whose yaw value must be matched.")]
        public Transform target;
        [Tooltip("Approximately how long it should take to rotate to match the target.")]
        [Min(.001f)] public float smoothTime = .1f;
        float yawVelocity;

        void OnEnable()
        {
            if (TryValidateRequiredField(this, target))
            {
                float yawGoal = Yaw(target.rotation);
                float yaw = Yaw(transform.rotation);
                RecenterDegrees(ref yaw, ref yawGoal);
                transform.Rotate(0, yawGoal - yaw, 0);
            }
            else
            {
                enabled = false;
            }
        }

        void LateUpdate()
        {
            float yawGoal = Yaw(target.rotation);
            float yaw = Yaw(transform.rotation);
            RecenterDegrees(ref yaw, ref yawGoal);
            float newYaw = Mathf.SmoothDamp(yaw, yawGoal, ref yawVelocity, smoothTime);
            transform.Rotate(0, newYaw - yaw, 0);
        }
    }
}