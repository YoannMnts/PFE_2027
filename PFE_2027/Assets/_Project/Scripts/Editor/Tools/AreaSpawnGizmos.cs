using System.Collections.Generic;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.RoadSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Editor._Project.Scripts.Editor.Tools
{
    // Scene view preview of the progressive spawn: for each spawn point of the edited area, a handle on the
    // pilgrim's path where it gets revealed (SpawnPoint.Progression). Dragging it moves the progression along
    // the path (projected on the nearest point of the spline, like PilgrimInstance does at runtime).
    public static class AreaSpawnGizmos
    {
        private const string SPAWN_POINTS_PATH = "<SpawnPoints>k__BackingField";
        private const string PROGRESSION_PATH = "<Progression>k__BackingField";

        private const float MARKER_RADIUS = 0.25f;
        private const float HANDLE_SIZE = 0.08f;
        private const float LABEL_HEIGHT = 0.8f;
        private const float LABEL_SPACING = 0.35f;

        private static readonly Color MarkerColor = new(0.35f, 0.8f, 1f);

        // Spawn points sharing a progression: their labels are stacked instead of overlapping
        private static readonly Dictionary<float, int> StackCounts = new();

        private static AreaData area;

        // SerializedObject of the edited area, recreated when another area is shown (supports Undo)
        private static SerializedObject serializedArea;

        public static void Show(AreaData editedArea)
        {
            area = editedArea;

            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            ObjectChangeEvents.changesPublished -= OnChangesPublished;
            ObjectChangeEvents.changesPublished += OnChangesPublished;

            SceneView.RepaintAll();
        }

        public static void Hide()
        {
            area = null;

            SceneView.duringSceneGui -= OnSceneGUI;
            ObjectChangeEvents.changesPublished -= OnChangesPublished;

            SceneView.RepaintAll();
        }

        // A progression edited in the tab panel: redraw right away
        private static void OnChangesPublished(ref ObjectChangeEventStream stream) => SceneView.RepaintAll();

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (area == null || area.SpawnPoints == null)
                return;

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
                return;

            bool isRepaint = Event.current.type == EventType.Repaint;

            RuntimeArea runtimeArea = stage.prefabContentsRoot.GetComponentInChildren<RuntimeArea>(true);
            SplineContainer path = runtimeArea != null ? runtimeArea.Path : null;
            if (path == null || path.Spline == null)
            {
                if (isRepaint)
                    Handles.Label(stage.prefabContentsRoot.transform.position, "No RuntimeArea path: spawn progression can't be shown");
                return;
            }

            if (serializedArea == null || serializedArea.targetObject != area)
            {
                serializedArea?.Dispose();
                serializedArea = new SerializedObject(area);
            }

            serializedArea.Update();
            SerializedProperty spawnPointsProperty = serializedArea.FindProperty(SPAWN_POINTS_PATH);

            StackCounts.Clear();
            List<SpawnPoint> spawnPoints = area.SpawnPoints;

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                SpawnPoint spawnPoint = spawnPoints[i];

                // Pilgrims are there from the start: their progression is not used
                if (spawnPoint.Npc is PilgrimData)
                    continue;

                Vector3 marker = path.EvaluatePosition(spawnPoint.Progression);

                if (isRepaint)
                    DrawMarker(spawnPoint, marker);

                // Handle: on every event (it must receive the mouse)
                float handleSize = HandleUtility.GetHandleSize(marker) * HANDLE_SIZE;
                using (new Handles.DrawingScope(MarkerColor))
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 moved = Handles.FreeMoveHandle(marker, handleSize, Vector3.zero, Handles.DotHandleCap);
                    if (EditorGUI.EndChangeCheck() && spawnPointsProperty != null)
                    {
                        SerializedProperty progression = spawnPointsProperty
                            .GetArrayElementAtIndex(i)
                            .FindPropertyRelative(PROGRESSION_PATH);

                        progression.floatValue = GetProgression(path, moved);
                        serializedArea.ApplyModifiedProperties();
                    }
                }
            }
        }

        // Same projection as PilgrimInstance: normalized position of the nearest point of the spline
        private static float GetProgression(SplineContainer path, Vector3 worldPosition)
        {
            float3 localPosition = path.transform.InverseTransformPoint(worldPosition);
            SplineUtility.GetNearestPoint(path.Spline, localPosition, out float3 _, out float t);
            return Mathf.Clamp01(t);
        }

        private static void DrawMarker(SpawnPoint spawnPoint, Vector3 marker)
        {
            using (new Handles.DrawingScope(MarkerColor))
                Handles.DrawWireDisc(marker, Vector3.up, MARKER_RADIUS);

            StackCounts.TryGetValue(spawnPoint.Progression, out int stack);
            StackCounts[spawnPoint.Progression] = stack + 1;

            string npcName = spawnPoint.Npc != null ? spawnPoint.Npc.name : "(no NPC)";
            Vector3 labelPosition = marker + Vector3.up * (LABEL_HEIGHT + stack * LABEL_SPACING);
            Handles.Label(labelPosition, $"{spawnPoint.AnchorId} · {npcName} · {spawnPoint.Progression:P0}");
        }
    }
}
