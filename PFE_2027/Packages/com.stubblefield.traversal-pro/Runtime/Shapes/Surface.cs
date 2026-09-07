using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Describes a surface point on a collider.
    /// </summary>
    [System.Serializable]
    public class Surface
    {
        /// <summary>
        /// The point on the surface.
        /// </summary>
        public Vector3 Point { get; private set; }
        /// <summary>
        /// The normal of the surface at the point.
        /// </summary>
        public Vector3 Normal { get; private set; } = Vector3.up;
        /// <summary>
        /// The Collider of this Surface.
        /// </summary>
        public Collider Collider { get; private set; }
        /// <summary>
        /// The Rigidbody of this surface.
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }
        /// <summary>
        /// The angle in degrees between <see cref="Normal"/> and the world up vector.
        /// </summary>
        public float SlopeDegrees { get; private set; }
        /// <summary>
        /// The static friction coefficient of this surface.
        /// </summary>
        public float StaticFriction { get; private set; } = DefaultStaticFriction;
        /// <summary>
        /// The dynamic friction coefficient of this surface.
        /// </summary>
        public float DynamicFriction { get; private set; } = DefaultDynamicFriction;
        /// <summary>
        /// The velocity of the center of mass of the Rigidbody.
        /// </summary>
        public Vector3 Velocity { get; private set; }
        /// <summary>
        /// The velocity of this point on the Rigidbody.
        /// </summary>
        public Vector3 PointVelocity { get; private set; }
        /// <summary>
        /// The velocity needed to move the point to where it would be next frame if it were pinned
        /// to the Rigidbody. This is different than <see cref="PointVelocity"/> which is on a tangent
        /// line to the Rigidbody and is the velocity of the point if it were released from the
        /// Rigidbody at this moment in time.
        /// </summary>
        public Vector3 PointSecantVelocity { get; private set; }
        /// <summary>
        /// The acceleration of the center of mass of the Rigidbody.
        /// </summary>
        public Vector3 Acceleration { get; private set; }
        /// <summary>
        /// The angular velocity of the Rigidbody in radians per second.
        /// </summary>
        public Vector3 AngularVelocity { get; private set; }

        /// <summary>
        /// Update the data for this surface.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <param name="collider"></param>
        /// <param name="defaultStaticFriction"></param>
        /// <param name="defaultDynamicFriction"></param>
        public void Update(
            Vector3 point,
            Vector3 normal,
            Collider collider,
            float defaultStaticFriction = DefaultStaticFriction,
            float defaultDynamicFriction = DefaultDynamicFriction)
        {
            Point = point;
            Normal = normal.normalized;
            Collider priorCollider = Collider;
            Collider = collider;
            SlopeDegrees = Vector3.Angle(Vector3.up, Normal);
            (StaticFriction, DynamicFriction) = GetFriction(Collider, defaultStaticFriction, defaultDynamicFriction);
            Rigidbody priorRigidbody = Rigidbody;
            Rigidbody = Collider ? Collider.attachedRigidbody : null;
            if (Rigidbody)
            {
                if (Collider != priorCollider
                    && (Rigidbody.transform.localScale != Vector3.one
                        || Rigidbody.transform.lossyScale != Vector3.one))
                {
                    LogWarning($"The Rigidbody '{Rigidbody.name}' should have a scale of (1, 1, 1) for itself and all parents.");
                }

                Vector3 priorVelocity = Velocity;
                Velocity = Rigidbody.linearVelocity;
                AngularVelocity = Rigidbody.angularVelocity;
                PointVelocity = Rigidbody.GetPointVelocity(Point);
                PointSecantVelocity = GetPointSecantVelocity(Rigidbody, Point, Time.deltaTime);
                if (Rigidbody == priorRigidbody)
                {
                    float deltaTime = Time.deltaTime;
                    Acceleration = (Velocity - priorVelocity) / deltaTime;
                }
            }
            else
            {
                Velocity = default;
                AngularVelocity = default;
                PointVelocity = default;
                PointSecantVelocity = default;
                Acceleration = default;
            }
        }

        /// <summary>
        /// Set all values of this Surface to default except: normal becomes Vector3.up,
        /// Flatness becomes 1, StaticDynamic becomes <see cref="Utility.DefaultStaticFriction"/>,
        /// and DynamicFriction becomes <see cref="DynamicFriction"/>.
        /// </summary>
        public void Clear()
        {
            Point = default;
            Normal = Vector3.up;
            Collider = default;
            SlopeDegrees = default;
            StaticFriction = DefaultStaticFriction;
            DynamicFriction = DefaultDynamicFriction;
            Rigidbody = default;
            Velocity = default;
            AngularVelocity = default;
            PointVelocity = default;
            PointSecantVelocity = default;
            Acceleration = default;
        }
        
        /// <summary>
        /// Calculates the velocity from a point to its future position over deltaTime if it remains pinned
        /// to the rigidbody. Accounts for both the velocity and angular velocity of the rigidbody. This differs
        /// from standard point velocity, which is the velocity a point would travel if it were immediately
        /// released from the rigidbody. Standard point velocity is tangential to the rigidbody center of mass.
        /// </summary>
        static Vector3 GetPointSecantVelocity(Rigidbody rb, Vector3 point, float deltaTime)
        {
            Vector3 deltaAngles = rb.angularVelocity * Mathf.Rad2Deg * deltaTime;
            Quaternion rotation = Quaternion.Euler(deltaAngles);
            Vector3 localPoint = point - rb.worldCenterOfMass;
            Vector3 futureLocalPoint = rotation * localPoint;
            Vector3 deltaPosition = futureLocalPoint - localPoint;
            return deltaPosition / deltaTime + rb.linearVelocity;
        }
    }
}