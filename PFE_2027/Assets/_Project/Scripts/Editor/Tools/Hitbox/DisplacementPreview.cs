using System;
using PFE.Core.Scripts.Curves;
using PFE.Core.Scripts.Enemy.Attacks;
using UnityEditor;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Hitbox
{
    // Preview of the Displacement windows in the HitboxStage: the path on the ground with one tick per physics step
    // (close ticks = slow, spread ticks = fast), a dot at the playhead, and the character moved along it by Sample.
    // Preview direction: the initial forward of the character (in game: where it faces when the window opens).
    public static class DisplacementPreview
    {
        private const float GROUND_OFFSET = 0.02f;
        private const float TICK_HALF_WIDTH = 0.08f;
        private const float DOT_SIZE = 0.06f;
        private const float LABEL_OFFSET = 0.3f;
        private const int MAX_TICKS = 300;

        private static readonly Color PathColor = new(0.85f, 0.45f, 0.75f);     // Same as the Displacement track

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        // Meters travelled at a normalized time of the clip, summing every Displacement window met so far
        public static float GetDistanceAt(HitboxData data, float normalizedTime)
        {
            float total = 0f;
            ReadOnlySpan<AdditionalWindow> windows = data.Windows;
            for (int i = 0; i < windows.Length; i++)
            {
                if (windows[i].Flag is Displacement displacement)
                    total += GetWindowDistance(displacement, windows[i].Window, normalizedTime);
            }

            return total;
        }

        // Meters travelled inside one window at a normalized time of the clip (same math as the runtime)
        private static float GetWindowDistance(in Displacement displacement, TimeWindow window, float normalizedTime)
        {
            float curveDuration = displacement.ProgressOverWindow.GetProgressDuration();
            float length = window.End - window.Start;
            if (curveDuration <= 0f || length <= 0f || displacement.Distance <= 0f || normalizedTime <= window.Start)
                return 0f;

            float local = Mathf.Min((normalizedTime - window.Start) / length, 1f);
            return displacement.ProgressOverWindow.EvaluateProgressDelta(0f, local * curveDuration) * displacement.Distance;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            HitboxStage stage = HitboxStage.Current;
            if (stage == null || stage.Data == null)
                return;

            HitboxData data = stage.Data;
            float playSeconds = data.PlaySeconds;
            float step = Time.fixedDeltaTime;

            // The stage places the character at the origin, facing +Z: the path stays fixed while it moves
            Vector3 origin = Vector3.up * GROUND_OFFSET;
            Vector3 axis = Vector3.forward;
            Vector3 side = Vector3.right * TICK_HALF_WIDTH;
            bool hasDisplacement = false;

            using (new Handles.DrawingScope(PathColor))
            {
                ReadOnlySpan<AdditionalWindow> windows = data.Windows;
                for (int i = 0; i < windows.Length; i++)
                {
                    if (windows[i].Flag is not Displacement displacement)
                        continue;

                    TimeWindow window = windows[i].Window;
                    float seconds = (window.End - window.Start) * playSeconds;
                    float distance = GetWindowDistance(displacement, window, window.End);
                    if (seconds <= 0f || distance <= 0f)
                        continue;

                    hasDisplacement = true;

                    // The previous windows already moved the character: this one starts where they end
                    Vector3 start = origin + axis * GetDistanceAt(data, window.Start);
                    Vector3 end = start + axis * distance;
                    Handles.DrawLine(start, end);

                    // One tick per physics step, as the runtime moves it
                    int ticks = Mathf.Min(Mathf.CeilToInt(seconds / step), MAX_TICKS);
                    for (int k = 0; k <= ticks; k++)
                    {
                        float time = window.Start + (window.End - window.Start) * Mathf.Min(k * step / seconds, 1f);
                        Vector3 point = start + axis * GetWindowDistance(displacement, window, time);
                        Handles.DrawLine(point - side, point + side);
                    }

                    Handles.Label(end + axis * LABEL_OFFSET, $"Displacement  {distance:0.0} m · {seconds:0.00} s · {ticks} steps");
                }

                if (!hasDisplacement)
                    return;

                Vector3 current = origin + axis * GetDistanceAt(data, stage.NormalizedTime);
                Handles.DrawSolidDisc(current, Vector3.up, HandleUtility.GetHandleSize(current) * DOT_SIZE);
            }
        }
    }
}
