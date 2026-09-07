using UnityEngine;
using UnityEngine.Assertions;
using Unity.Mathematics;

namespace TraversalPro
{
    public static partial class Utility
    {
        internal const float minVectorMagnitude = .00001f;

        internal static Vector3 Abs(Vector3 value)
        {
            return new Vector3(
                Mathf.Abs(value.x),
                Mathf.Abs(value.y),
                Mathf.Abs(value.z));
        }
        
        static Vector3 AngleAxisScaled(Quaternion orientation)
        {
            orientation.ToAngleAxis(out float angle, out Vector3 axis);
            return axis * (angle * Mathf.Deg2Rad);
        }
        
        internal static void AssertNormalized(Vector3 vector)
        {
            Assert.AreApproximatelyEqual(1, vector.sqrMagnitude, 5E-7f, "Vector must be normalized.");
        }

        internal static float ClampToRange(float value, float center, float radius)
        {
            return Mathf.Clamp(value, center - radius, center + radius);
        }
        
        internal static Vector2 ClampToCircle(Vector2 value, Vector2 center, float radius)
        {
            Vector2 offset = value - center;
            float magnitude = offset.magnitude;
            Vector2 direction = magnitude > 0 ? offset / magnitude : default;
            float clampedMagnitude = magnitude < radius ? magnitude : radius;
            return center + direction * clampedMagnitude;
        }
        
        internal static Vector3 ClampToSphere(Vector3 value, Vector3 center, float radius)
        {
            Vector3 offset = value - center;
            float magnitude = offset.magnitude;
            Vector3 direction = offset / magnitude;
            float clampedMagnitude = magnitude < radius ? magnitude : radius;
            return center + direction * clampedMagnitude;
        }
        
        /// <summary>
        /// Decompose the given Quaternion into 3 angles in degrees around the z, x, and y axes in that order.
        /// </summary>
        internal static Vector3 EulerZXY(Quaternion orientation)
        {
            orientation.Normalize();
            Vector3 forward = orientation * Vector3.forward;
            Vector3 up = orientation * Vector3.up;
            bool isForwardVertical = forward.y > .9999f | forward.y < -.9999f;
            Vector3 yForward = isForwardVertical ? up : forward;
            float y = Mathf.Atan2(yForward.z, yForward.x) * Mathf.Rad2Deg * -1 + 90;
            float x = Vector3.Angle(Vector3.up, forward) - 90;
            Vector3 zUp = Quaternion.Euler(-x, 0, 0) * (Quaternion.Euler(0, -y, 0) * up);
            float z = Mathf.Atan2(zUp.y, zUp.x) * Mathf.Rad2Deg - 90;
            return new Vector3(x, y, z);
        }
        
        
        internal static float FixNaNs(float value, float replacement = 0)
        {
            return float.IsFinite(value) ? value : replacement;
        }
        
        internal static Vector2 FixNaNs(Vector2 value, float replacement = 0)
        {
            return new Vector2(
                float.IsFinite(value.x) ? value.x : replacement,
                float.IsFinite(value.y) ? value.y : replacement);
        } 
        
        internal static Vector3 FixNaNs(Vector3 value, float replacement = 0)
        {
            return new Vector3(
                float.IsFinite(value.x) ? value.x : replacement,
                float.IsFinite(value.y) ? value.y : replacement,
                float.IsFinite(value.z) ? value.z : replacement);
        }
        
        internal static Vector3 InsertY(this Vector2 value, float y = default)
        {
            return new Vector3(value.x, y, value.y);
        }
        
        internal static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) & float.IsFinite(value.y) & float.IsFinite(value.z);
        }

        internal static bool IsNormalized(Vector3 value)
        {
            const float delta = 5E-7f;
            float sqrMag = value.sqrMagnitude;
            return sqrMag >= 1 - delta & sqrMag <= 1 + delta;
        }
        
        internal static Vector2 Max(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x > b.x ? a.x : b.x,
                a.y > b.y ? a.y : b.y);
        }
        
        internal static Vector3 Max(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x > b.x ? a.x : b.x,
                a.y > b.y ? a.y : b.y,
                a.z > b.z ? a.z : b.z);
        }

        internal static Vector2 Min(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x < b.x ? a.x : b.x,
                a.y < b.y ? a.y : b.y);
        }

        internal static Vector3 Min(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.x < b.x ? a.x : b.x,
                a.y < b.y ? a.y : b.y,
                a.z < b.z ? a.z : b.z);
        }
        
        internal static Vector3 MoveDelta(Vector3 current, Vector3 target, float maxDeltaMagnitude)
        {
            Vector3 offset = target - current;
            float sqrMag = offset.sqrMagnitude;
            maxDeltaMagnitude = maxDeltaMagnitude < 0 ? 0 : maxDeltaMagnitude;
            if (sqrMag <= maxDeltaMagnitude * maxDeltaMagnitude) return offset;
            float mag = Mathf.Sqrt(sqrMag);
            return offset / mag * maxDeltaMagnitude;
        }
        
        /// <summary>
        /// Positive pitch angles rotate upwards.
        /// </summary>
        /// <param name="orientation"></param>
        /// <returns>In degrees.</returns>
        internal static float Pitch(Quaternion orientation)
        {
            return -(Vector3.Angle(Vector3.up, orientation * Vector3.forward) - 90);
        }
        
        internal static void RecenterDegrees(ref float yawA, ref float yawB)
        {
            yawA %= 360;
            yawB %= 360;
            float diff = yawA - yawB;
            if (diff > 180)
            {
                yawA -= 360;
            }
            else if (diff < -180)
            {
                yawA += 360;
            }
        }
        
        internal static Vector3 Rejection(Vector3 vector, Vector3 onNormal)
        {
            return vector - Vector3.Project(vector, onNormal);
        }
        
        /// <summary>
        /// Angle is in degrees. Positive angles rotate counter clockwise.
        /// </summary>
        internal static Vector2 Rotate2d(Vector2 value, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(
                value.x * cos - value.y * sin,
                value.x * sin + value.y * cos);
        }

        internal static Vector3 RotateFlat(Vector3 value)
        {
            if (value == default) return value;
            float mag = value.magnitude;
            Vector3 direction = value / mag;
            direction.y = 0;
            direction.Normalize();
            return direction * mag;
        }

        internal static Vector3 RotateHorizontally(Vector3 value, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector3(
                value.x * cos - value.z * sin,
                value.y,
                value.x * sin + value.z * cos);
        }

        /// <summary>
        /// Angle is in degrees. Positive angles rotate downward.
        /// </summary>
        internal static Vector3 RotateVertically(Vector3 value, float degrees)
        {
            Vector3 axis = Vector3.Cross(Vector3.up, value);
            if (axis.sqrMagnitude < minVectorMagnitude) return value;
            Quaternion rotation = Quaternion.AngleAxis(degrees, axis);
            return rotation * value;
        }
        
        internal static Vector3 Round(Vector3 value)
        {
            return new Vector3(
                Mathf.Round(value.x),
                Mathf.Round(value.y),
                Mathf.Round(value.z));
        }

        internal static Vector3 RoundToTolerance(Vector3 value, float toleranceMultiplier = 10000)
        {
            value *= toleranceMultiplier;
            value = Round(value);
            value /= toleranceMultiplier;
            return value;
        }

        internal static void SetXZ(ref this Vector3 value, Vector2 xz)
        {
            value.x = xz.x;
            value.z = xz.y;
        }
        
        /// <summary>
        /// The angle in degrees between the up vector and the given normal vector. 
        /// </summary>
        internal static float SlopeDegrees(Vector3 normal)
        {
            return Vector3.Angle(Vector3.up, normal);
        }
        
        static Quaternion SmallestDifference(Quaternion a, Quaternion b)
        {
            if (Quaternion.Dot(a, b) < 0) b = new Quaternion(-b.x, -b.y, -b.z, -b.w);
            return a * Quaternion.Inverse(b);
        }

        internal static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Vector3 angularVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            Quaternion quaternionOffset = SmallestDifference(current, target);
            Vector3 angleAxisOffset0 = AngleAxisScaled(quaternionOffset);
            Vector3 angleAxisOffset1 = Vector3.SmoothDamp(angleAxisOffset0, default, ref angularVelocity, smoothTime, maxSpeed, deltaTime);
            Offset3 angleAxisOffsetDiff = new(angleAxisOffset1 - angleAxisOffset0);
            Quaternion rotation = Quaternion.AngleAxis(angleAxisOffsetDiff.Magnitude * Mathf.Rad2Deg, angleAxisOffsetDiff.Direction);
            return rotation * current;
        }
        
        internal static Vector3 SmoothDampAcceleration(Vector3 current, Vector3 target, Vector3 velocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            double3 v0 = new(velocity.x, velocity.y, velocity.z);
            Vector3.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);
            double3 v1 = new(velocity.x, velocity.y, velocity.z);
            double3 acceleration = (v1 - v0) / deltaTime;
            return new Vector3((float)acceleration.x, (float)acceleration.y, (float)acceleration.z);
        }
        
        internal static Vector3 SmoothDampAcceleration(Quaternion current, Quaternion target, Vector3 angularVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            Quaternion quaternionOffset = SmallestDifference(current, target);
            Vector3 angleAxisOffset0 = AngleAxisScaled(quaternionOffset);
            Vector3 angularVelocity1 = angularVelocity;
            Vector3.SmoothDamp(angleAxisOffset0, default, ref angularVelocity1, smoothTime, maxSpeed, deltaTime);
            return (angularVelocity1 - angularVelocity) / Time.deltaTime;
        }
        
        internal static Vector3 SnapPointToPlaneVertically(Vector3 point, Vector3 planeNormal)
        {
            // it's ok if planeNormal is not normalized, the math still works
            if (planeNormal.y == 0) return point;
            point.y = (-point.x * planeNormal.x - point.z * planeNormal.z) / planeNormal.y;
            return point;
        }
        
        internal static Plane VelocityPlane(Vector3 velocity)
        {
            Vector3 cross = Vector3.Cross(Vector3.up, velocity);
            Vector3 normal = -Vector3.Cross(cross, velocity);
            return new Plane(normal, 0);
        }
        
        internal static Vector2 WorldToLocalDirection(Vector3 worldDirection, float worldYaw)
        {
            return Rotate2d(worldDirection, worldYaw);
        }
        
        /// <summary>
        /// Gets the yaw value in degrees for the given direction.
        /// </summary>
        internal static float Yaw(Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg * -1 + 90;
        }

        internal static float Yaw(Quaternion orientation)
        {
            orientation.Normalize();
            Vector3 forward = orientation * Vector3.forward;
            if (forward.y > .9999f)
            {
                forward = orientation * -Vector3.up;
            }
            else if (forward.y < -.9999f)
            {
                forward = orientation * Vector3.up;
            }
            return Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg * -1 + 90;
        }
        
        internal static Vector3 WithXZ(this Vector3 value, Vector2 xz)
        {
            value.x = xz.x;
            value.z = xz.y;
            return value;
        }

        internal static Vector3 WithY(this Vector3 value, float y)
        {
            return new Vector3(value.x, y, value.z);
        }

        internal static Vector2 XZ(this Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }
    }
}