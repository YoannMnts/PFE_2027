using System;
using UnityEngine;

namespace TraversalPro
{
    public static partial class Utility
    {
        internal static void DrawCapsuleGizmo(in Capsule capsule)
        {
            DrawCapsuleGizmo(capsule.lower, capsule.upper, capsule.radius);
        }

        internal static void DrawCapsuleGizmo(Vector3 point1, Vector3 point2, float radius)
        {
            Vector3 upOffset = point2 - point1;
            if (upOffset == default)
            {
                DrawCircleGizmo(point1, radius, Quaternion.identity, 32, 32);
                DrawCircleGizmo(point1, radius, Quaternion.LookRotation(Vector3.right, Vector3.up), 32, 32);
                DrawCircleGizmo(point1, radius, Quaternion.LookRotation(Vector3.up, Vector3.back), 32, 32);
                return;
            }
            Vector3 up = upOffset.normalized;
            Quaternion orientation = Quaternion.FromToRotation(Vector3.up, up);
            Vector3 right = orientation * Vector3.right;
            Vector3 forward = orientation * Vector3.forward;
            DrawRayGizmo(point1 + right * radius, upOffset);
            DrawRayGizmo(point1 - right * radius, upOffset);
            DrawRayGizmo(point1 + forward * radius, upOffset);
            DrawRayGizmo(point1 - forward * radius, upOffset);
            DrawCircleGizmo(point2, radius, Quaternion.LookRotation(up, -forward), 32, 32);
            DrawCircleGizmo(point1, radius, Quaternion.LookRotation(up, -forward), 32, 32);
            DrawCircleGizmo(point2, radius, Quaternion.LookRotation(forward, up), 16, 32);
            DrawCircleGizmo(point2, radius, Quaternion.LookRotation(right, up), 16, 32);
            DrawCircleGizmo(point1, radius, Quaternion.LookRotation(forward, -up), 16, 32);
            DrawCircleGizmo(point1, radius, Quaternion.LookRotation(right, -up), 16, 32);
        }
        
        internal static void DrawRayGizmo(Vector3 origin, Vector3 offset)
        {
            Gizmos.DrawLine(origin, origin + offset);
        }

        internal static void DrawCircleGizmo(Vector3 center, float radius, Quaternion orientation, int drawSegments, int totalSegments)
        {
            Span<Vector3> points = stackalloc Vector3[drawSegments + 1];
            for (int i = 0; i <= drawSegments; i++)
            {
                float angle = (float)i / totalSegments * Mathf.PI * 2;
                Vector3 unitOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                points[i] = center + orientation * unitOffset * radius;
            }
            Gizmos.DrawLineStrip(points, false);
        }
    }
}