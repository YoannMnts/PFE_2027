using System;
using PFE.Core.Scripts.Enemy.Attacks;
using UnityEditor;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    public static class HitboxDrawer
    {
        private static readonly Color[] Palette =
        {
            new(1f, 0.35f, 0.25f),
            new(0.3f, 0.8f, 1f),
            new(1f, 0.8f, 0.2f),
            new(0.6f, 1f, 0.4f),
            new(0.9f, 0.45f, 1f),
        };

        private const float INACTIVE_ALPHA = 0.25f;
        private const float FILL_ALPHA = 0.2f;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;

            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            ObjectChangeEvents.changesPublished += OnChangesPublished;
        }

        private static void OnChangesPublished(ref ObjectChangeEventStream stream)
        {
            if (AttackStage.Current != null)
                SceneView.RepaintAll();
        }

        public static Color GetColor(int index) => Palette[index % Palette.Length];

        private static void OnSceneGUI(SceneView sceneView)
        {
            AttackStage stage = AttackStage.Current;
            if (stage == null || stage.Attack == null || stage.Character == null)
                return;

            Event current = Event.current;
            bool isRepaint = current.type == EventType.Repaint;

            HitboxAnchors anchors = stage.Character.GetComponentInChildren<HitboxAnchors>();
            if (anchors == null)
            {
                if (isRepaint)
                    Handles.Label(stage.Character.transform.position, "No HitboxAnchors component on this character");
                return;
            }

            ReadOnlySpan<HitboxWindow> hitboxes = stage.Attack.Hitboxes;

            // The selected hitbox was removed from the array
            if (stage.SelectedHitbox >= hitboxes.Length)
                stage.SelectedHitbox = -1;

            // Escape: stop editing the hitbox
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape && stage.SelectedHitbox >= 0)
            {
                stage.SelectedHitbox = -1;
                current.Use();
            }

            // Our handles replace Unity's while a hitbox is selected
            UnityEditor.Tools.hidden = stage.SelectedHitbox >= 0;

            float time = stage.NormalizedTime;

            for (int i = 0; i < hitboxes.Length; i++)
            {
                ref readonly HitboxWindow hitbox = ref hitboxes[i];

                if (!anchors.TryGet(hitbox.Anchor, out Transform bone))
                {
                    if (isRepaint)
                    {
                        string anchorName = hitbox.Anchor != null ? hitbox.Anchor.name : "?";
                        Handles.Label(anchors.transform.position + Vector3.up * (2f + 0.2f * i), $"#{i}: missing anchor '{anchorName}'");
                    }
                    continue;
                }

                HitboxMath.GetPose(bone, in hitbox, out Vector3 center, out Quaternion rotation);
                bool selected = i == stage.SelectedHitbox;

                // Drawing: on Repaint only
                if (isRepaint)
                {
                    bool active = hitbox.Window.Contains(time);
                    DrawHitbox(in hitbox, center, rotation, GetColor(i), active || selected);
                    Handles.Label(center, $"#{i}");
                }

                // Clickable dot at the center: on every event (it must receive the click)
                if (!selected)
                {
                    float dotSize = HandleUtility.GetHandleSize(center) * 0.06f;
                    using (new Handles.DrawingScope(GetColor(i)))
                    {
                        if (Handles.Button(center, Quaternion.identity, dotSize, dotSize * 1.5f, Handles.DotHandleCap))
                            stage.SelectedHitbox = i;
                    }
                }
                else
                {
                    HitboxHandles.Edit(stage, i, bone);
                }
            }
        }

        private static void DrawHitbox(in HitboxWindow hitbox, Vector3 center, Quaternion rotation, Color color, bool active)
        {
            Color wire = active ? color : WithAlpha(color, INACTIVE_ALPHA);
            Color fill = WithAlpha(color, FILL_ALPHA);

            switch (hitbox.Shape)
            {
                case HitboxShape.Box:
                    using (new Handles.DrawingScope(wire, Matrix4x4.TRS(center, rotation, Vector3.one)))
                        Handles.DrawWireCube(Vector3.zero, hitbox.Size);

                    if (active)
                    {
                        using (new Handles.DrawingScope(fill, Matrix4x4.TRS(center, rotation, hitbox.Size)))
                            Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.Repaint);
                    }
                    break;

                case HitboxShape.Sphere:
                    float radius = hitbox.Size.x;
                    using (new Handles.DrawingScope(wire, Matrix4x4.TRS(center, rotation, Vector3.one)))
                    {
                        Handles.DrawWireDisc(Vector3.zero, Vector3.up, radius);
                        Handles.DrawWireDisc(Vector3.zero, Vector3.right, radius);
                        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, radius);
                    }

                    if (active)
                    {
                        using (new Handles.DrawingScope(fill))
                            Handles.SphereHandleCap(0, center, rotation, radius * 2f, EventType.Repaint);   // size = diameter
                    }
                    break;

                case HitboxShape.Capsule:
                    DrawWireCapsule(center, rotation, hitbox.Size, wire);
                    break;
            }
        }
        private static void DrawWireCapsule(Vector3 center, Quaternion rotation, Vector3 size, Color color)
        {
            HitboxMath.GetCapsule(center, rotation, size, out Vector3 point0, out Vector3 point1, out float radius);
            float half = Vector3.Distance(point0, point1) * 0.5f;

            using (new Handles.DrawingScope(color, Matrix4x4.TRS(center, rotation, Vector3.one)))
            {
                Vector3 top = Vector3.up * half;
                Vector3 bottom = -top;

                Handles.DrawWireDisc(top, Vector3.up, radius);
                Handles.DrawWireDisc(bottom, Vector3.up, radius);

                Handles.DrawLine(top + Vector3.right * radius, bottom + Vector3.right * radius);
                Handles.DrawLine(top - Vector3.right * radius, bottom - Vector3.right * radius);
                Handles.DrawLine(top + Vector3.forward * radius, bottom + Vector3.forward * radius);
                Handles.DrawLine(top - Vector3.forward * radius, bottom - Vector3.forward * radius);

                Handles.DrawWireArc(top, Vector3.forward, Vector3.right, 180f, radius);
                Handles.DrawWireArc(top, Vector3.right, -Vector3.forward, 180f, radius);
                Handles.DrawWireArc(bottom, Vector3.forward, -Vector3.right, 180f, radius);
                Handles.DrawWireArc(bottom, Vector3.right, Vector3.forward, 180f, radius);
            }
        }
        
        private static Color WithAlpha(Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}