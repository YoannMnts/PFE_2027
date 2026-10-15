using PFE.Core.Scripts.Enemy.Attacks;
using UnityEditor;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Hitbox
{
    // Draws and edits the knockback redirect cone of the PushbackData opened in the HitboxStage.
    // Preview: the character is hit from the front, so the knockback (and the cone) points behind it.
    public static class KnockbackRedirectHandles
    {
        private const string RANGE_PATH = "<Range>k__BackingField";
        private const string HALF_ANGLE_PATH = "<HalfAngle>k__BackingField";
        private const float MAX_HALF_ANGLE = 90f;
        private const float GROUND_OFFSET = 0.02f;
        private const float HANDLE_SIZE = 0.08f;

        private static readonly Color WireColor = new(1f, 0.6f, 0.15f);
        private static readonly Color FillColor = new(1f, 0.6f, 0.15f, 0.12f);

        // SerializedObject of the edited cone, recreated when another asset is shown
        private static SerializedObject serializedRedirect;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            HitboxStage stage = HitboxStage.Current;
            if (stage == null || stage.Character == null || stage.Data is not PushbackData pushback || pushback.Redirect == null)
                return;

            KnockbackRedirectData redirect = pushback.Redirect;
            if (serializedRedirect == null || serializedRedirect.targetObject != redirect)
            {
                serializedRedirect?.Dispose();
                serializedRedirect = new SerializedObject(redirect);
            }

            serializedRedirect.Update();
            SerializedProperty rangeProperty = serializedRedirect.FindProperty(RANGE_PATH);
            SerializedProperty halfAngleProperty = serializedRedirect.FindProperty(HALF_ANGLE_PATH);

            float range = rangeProperty.floatValue;
            float halfAngle = halfAngleProperty.floatValue;

            Transform root = stage.Character.transform;
            Vector3 origin = root.position + Vector3.up * GROUND_OFFSET;
            Vector3 axis = -root.forward;
            axis.y = 0f;
            axis = axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.back;

            Vector3 left = Quaternion.AngleAxis(-halfAngle, Vector3.up) * axis;
            Vector3 right = Quaternion.AngleAxis(halfAngle, Vector3.up) * axis;

            if (Event.current.type == EventType.Repaint)
                Draw(origin, axis, left, right, range, halfAngle);

            float handleSize = HandleUtility.GetHandleSize(origin + axis * range) * HANDLE_SIZE;
            using (new Handles.DrawingScope(WireColor))
            {
                // Range: dot at the tip, slides along the axis
                EditorGUI.BeginChangeCheck();
                Vector3 tip = Handles.Slider(origin + axis * range, axis, handleSize, Handles.DotHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    rangeProperty.floatValue = Mathf.Max(0f, Vector3.Dot(tip - origin, axis));
                    serializedRedirect.ApplyModifiedProperties();
                }

                // Half angle: dot on the right edge, its angle to the axis on the ground plane
                EditorGUI.BeginChangeCheck();
                Vector3 edge = Handles.FreeMoveHandle(origin + right * range, handleSize, Vector3.zero, Handles.DotHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 flat = edge - origin;
                    flat.y = 0f;
                    halfAngleProperty.floatValue = Mathf.Clamp(Vector3.Angle(axis, flat), 0f, MAX_HALF_ANGLE);
                    serializedRedirect.ApplyModifiedProperties();
                }
            }
        }

        private static void Draw(Vector3 origin, Vector3 axis, Vector3 left, Vector3 right, float range, float halfAngle)
        {
            using (new Handles.DrawingScope(FillColor))
                Handles.DrawSolidArc(origin, Vector3.up, left, halfAngle * 2f, range);

            using (new Handles.DrawingScope(WireColor))
            {
                Handles.DrawWireArc(origin, Vector3.up, left, halfAngle * 2f, range);
                Handles.DrawLine(origin, origin + left * range);
                Handles.DrawLine(origin, origin + right * range);
                Handles.DrawDottedLine(origin, origin + axis * range, 4f);
            }

            Handles.Label(origin + axis * (range + 0.3f),
                $"Knockback redirect  {range:0.0} m · {halfAngle * 2f:0}°\n(preview: hit from the front)");
        }
    }
}
