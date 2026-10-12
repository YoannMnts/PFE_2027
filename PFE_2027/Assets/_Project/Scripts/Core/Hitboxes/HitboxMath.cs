using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public static class HitboxMath
    {
        public static void GetPose(Transform anchor, in HitboxWindow hitbox, out Vector3 center, out Quaternion rotation)
        {
            Quaternion anchorRotation = anchor.rotation;
            rotation = anchorRotation * Quaternion.Euler(hitbox.Rotation);
            center = anchor.position + anchorRotation * hitbox.Offset;
        }

        public static void GetCapsule(Vector3 center, Quaternion rotation, Vector3 size, out Vector3 point0, out Vector3 point1, out float radius)
        {
            radius = size.x;
            float halfSegment = Mathf.Max(0f, size.y * 0.5f - radius);
            Vector3 axis = rotation * Vector3.up * halfSegment;
            point0 = center - axis;
            point1 = center + axis;
        }
    }
}