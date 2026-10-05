using PFE.Core.Scripts.Enemy.Attacks;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    // Poignées d'édition de la hitbox sélectionnée, selon l'outil actif de Unity (W / E / R),
    // en respectant le réglage Local/Global de la barre d'outils.
    public static class HitboxHandles
    {
        // Réutilisées : elles gardent leur état interne (face attrapée) pendant le glisser
        private static readonly BoxBoundsHandle BoxHandle = new();
        private static readonly SphereBoundsHandle SphereHandle = new();
        private static readonly CapsuleBoundsHandle CapsuleHandle = new() { heightAxis = CapsuleBoundsHandle.HeightAxis.Y };

        public static void Edit(AttackStage stage, int index, Transform anchor)
        {
            SerializedObject so = stage.SerializedAttack;
            so.Update();

            SerializedProperty hitbox = so.FindProperty(AttackPropertyPaths.Hitboxes).GetArrayElementAtIndex(index);
            SerializedProperty offsetProperty = hitbox.FindPropertyRelative(AttackPropertyPaths.Offset);
            SerializedProperty rotationProperty = hitbox.FindPropertyRelative(AttackPropertyPaths.Rotation);
            SerializedProperty sizeProperty = hitbox.FindPropertyRelative(AttackPropertyPaths.Size);

            // Même géométrie que le runtime
            HitboxWindow data = stage.Attack.Hitboxes[index];
            HitboxMath.GetPose(anchor, in data, out Vector3 center, out Quaternion rotation);
            Quaternion toAnchorSpace = Quaternion.Inverse(anchor.rotation);

            switch (UnityEditor.Tools.current)
            {
                case Tool.Move:
                {
                    Quaternion handleRotation = UnityEditor.Tools.pivotRotation == PivotRotation.Local ? rotation : Quaternion.identity;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newCenter = Handles.PositionHandle(center, handleRotation);
                    if (EditorGUI.EndChangeCheck())
                    {
                        offsetProperty.vector3Value = toAnchorSpace * (newCenter - anchor.position);
                        so.ApplyModifiedProperties();
                    }
                    break;
                }

                case Tool.Rotate:
                {
                    EditorGUI.BeginChangeCheck();
                    Quaternion newRotation = Handles.RotationHandle(rotation, center);
                    if (EditorGUI.EndChangeCheck())
                    {
                        rotationProperty.vector3Value = (toAnchorSpace * newRotation).eulerAngles;
                        so.ApplyModifiedProperties();
                    }
                    break;
                }

                case Tool.Scale:
                case Tool.Rect:
                {
                    Vector3 size = data.Size;
                    Vector3 newSize = size;
                    Vector3 centerShift = Vector3.zero; // déplacement du centre, dans le repère de la hitbox

                    using (new Handles.DrawingScope(Matrix4x4.TRS(center, rotation, Vector3.one)))
                    {
                        EditorGUI.BeginChangeCheck();
                        switch (data.Shape)
                        {
                            case HitboxShape.Box:
                                BoxHandle.center = Vector3.zero;
                                BoxHandle.size = size;
                                BoxHandle.DrawHandle();
                                newSize = BoxHandle.size;
                                centerShift = BoxHandle.center;
                                break;

                            case HitboxShape.Sphere:
                                SphereHandle.center = Vector3.zero;
                                SphereHandle.radius = size.x;
                                SphereHandle.DrawHandle();
                                newSize = new Vector3(SphereHandle.radius, size.y, size.z);
                                centerShift = SphereHandle.center;
                                break;

                            case HitboxShape.Capsule:
                                CapsuleHandle.center = Vector3.zero;
                                CapsuleHandle.radius = size.x;
                                CapsuleHandle.height = size.y;
                                CapsuleHandle.DrawHandle();
                                newSize = new Vector3(CapsuleHandle.radius, CapsuleHandle.height, size.z);
                                centerShift = CapsuleHandle.center;
                                break;
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            sizeProperty.vector3Value = newSize;
                            // une face tirée déplace le centre : on le répercute sur l'offset
                            Vector3 newCenter = center + rotation * centerShift;
                            offsetProperty.vector3Value = toAnchorSpace * (newCenter - anchor.position);
                            so.ApplyModifiedProperties();
                        }
                    }
                    break;
                }
            }
        }
    }
}
