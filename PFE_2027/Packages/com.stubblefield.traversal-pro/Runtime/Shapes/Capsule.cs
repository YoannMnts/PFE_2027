using System;
using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// A geometrical capsule shape defined by two 3D points in space and a radius. This can be
    /// constructed from an existing CapsuleCollider.
    /// </summary>
    [System.Serializable]
    internal struct Capsule
    {
        /// <summary>
        /// The lower sphere center of this capsule.
        /// </summary>
        public Vector3 lower;
        /// <summary>
        /// The upper sphere center of this capsule.
        /// </summary>
        public Vector3 upper;
        /// <summary>
        /// The radius of this capsule.
        /// </summary>
        public float radius;

        /// <summary>
        /// The point exactly in between the upper and lower sphere center points.
        /// </summary>
        public Vector3 Center
        {
            readonly get => (lower + upper) * .5f;
            set
            {
                Vector3 offset = value - Center;
                lower += offset;
                upper += offset;
            }
        }

        /// <summary>
        /// The distance from one tip of the capsule to the other tip.
        /// </summary>
        /// <returns></returns>
        public readonly float GetLength() => (upper - lower).magnitude + radius + radius;

        /// <summary>
        /// Construct a Capsule from an existing CapsuleCollider.
        /// </summary>
        /// <param name="collider"></param>
        public Capsule(CapsuleCollider collider) 
            : this(collider, collider.transform.TransformPoint(collider.center)) { }

        public Capsule(CapsuleCollider collider, Vector3 center)
        {
            int axisA = collider.direction;
            int axisB = (axisA + 1) % 3;
            int axisC = (axisA + 2) % 3;
            Vector3 lossyScale = Abs(collider.transform.lossyScale);
            float height = lossyScale[axisA] * collider.height;
            radius = Mathf.Max(lossyScale[axisB], lossyScale[axisC]) * collider.radius;
            Vector3 direction = default;
            direction[axisA] = 1;
            direction = collider.transform.TransformDirection(direction);
            float cylinderHeight = Mathf.Max(height, radius * 2) - radius * 2;
            lower = center - direction * cylinderHeight * .5f;
            upper = center + direction * cylinderHeight * .5f;
        }

        public readonly Capsule WithRadius(float radius)
        {
            return new Capsule()
            {
                lower = lower,
                upper = upper,
                radius = radius,
            };
        }

        /// <summary>
        /// Gets the sphere center with the larger y value.
        /// </summary>
        /// <returns></returns>
        public readonly Vector3 GetUpperCenter()
        {
            return upper.y > lower.y ? upper : lower;
        }

        /// <summary>
        /// Gets the sphere center with the lesser y value.
        /// </summary>
        /// <returns></returns>
        public readonly Vector3 GetLowerCenter()
        {
            return upper.y > lower.y ? lower : upper;
        }

        /// <summary>
        /// Gets the tip with the greater y value.
        /// </summary>
        /// <returns></returns>
        public readonly Vector3 GetUpperTip()
        {
            return GetUpperCenter() + Vector3.up * radius;
        }

        /// <summary>
        /// Gets the tip with the lesser y value.
        /// </summary>
        /// <returns></returns>
        public readonly Vector3 GetLowerTip()
        {
            return GetLowerCenter() - Vector3.up * radius;
        }

        /// <summary>
        /// Is the point contained in this Capsule?
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public readonly bool Contains(Vector3 point)
        {
            if (upper.Equals(lower))
            {
                return (point - lower).sqrMagnitude < radius * radius;
            }
            Vector3 segmentVector = upper - lower;
            Vector3 localPoint = point - lower;
            float projectionValue = Mathf.Clamp01(Vector3.Dot(segmentVector, localPoint) / segmentVector.sqrMagnitude);
            Vector3 pointOnSegment = segmentVector * projectionValue;
            return (localPoint - pointOnSegment).sqrMagnitude < radius * radius;
        }
        
        /// <summary>
        /// Gets the nearest point on the surface of this capsule to a given point.
        /// </summary>
        /// <param name="point">The given point.</param>
        /// <param name="surfacePoint">The nearest point on the surface of this capsule to the given point.</param>
        /// <param name="normal">The normal of the point on the surface of this capsule.</param>
        /// <param name="distance">The distance from the surface point to the given point. The value will be positive
        /// if the given point is outside the capsule and negative if inside the capsule.</param>
        public readonly void GetSurfacePoint(Vector3 point, out Vector3 surfacePoint, out Vector3 normal, out float distance)
        {
            Vector3 segmentVector = upper - lower;
            Vector3 localPoint = point - lower;
            float segmentPercent = lower.Equals(upper) ? 0 : Mathf.Clamp01(Vector3.Dot(segmentVector, localPoint) / segmentVector.sqrMagnitude);
            Vector3 localPointProjected = segmentVector * segmentPercent;
            Vector3 pointOffset = localPoint - localPointProjected;
            float pointDistance = pointOffset.magnitude;
            normal = pointDistance > 0 ? pointOffset / pointDistance : default;
            surfacePoint = lower + localPointProjected + normal * radius;
            distance = pointDistance - radius;
        }
        
        public bool Equals(Capsule other)
        {
            return lower.Equals(other.lower) && upper.Equals(other.upper) && radius.Equals(other.radius);
        }

        public override bool Equals(object obj)
        {
            return obj is Capsule other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(lower, upper, radius);
        }

        public static bool operator ==(Capsule a, Capsule b) => a.Equals(b);

        public static bool operator !=(Capsule a, Capsule b) => !a.Equals(b);
        
        public override string ToString()
        {
            return $"lower {lower} upper {upper} radius {radius}";
        }
        
        public string ToString(string format)
        {
            return $"lower {lower.ToString(format)} upper {upper.ToString(format)} radius {radius.ToString(format)}";
        }
    }
}