using System.Collections.Generic;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Editor._Project.Scripts.Editor.Tools
{
    // Scene view preview of the progressive spawn: for each spawn point of the edited area, a marker on the
    // pilgrim's path where it gets revealed (SpawnPoint.Progression), linked to the anchor it spawns on.
    public static class AreaSpawnGizmos
    {
        private const float MARKER_RADIUS = 0.25f;
        private const float LABEL_HEIGHT = 0.8f;
        private const float LABEL_SPACING = 0.35f;
        private const float DASH_SIZE = 4f;

        private static readonly Color MarkerColor = new(0.35f, 0.8f, 1f);
        private static readonly Color LinkColor = new(0.35f, 0.8f, 1f, 0.5f);

        // Spawn points sharing a progression: their labels are stacked instead of overlapping
        private static readonly Dictionary<float, int> StackCounts = new();

        private static AreaData area;

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
            if (Event.current.type != EventType.Repaint || area == null || area.SpawnPoints == null)
                return;

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
                return;

            RuntimeArea runtimeArea = stage.prefabContentsRoot.GetComponentInChildren<RuntimeArea>(true);
            SplineContainer path = runtimeArea != null ? runtimeArea.Path : null;
            if (path == null || path.Spline == null)
            {
                Handles.Label(stage.prefabContentsRoot.transform.position, "No RuntimeArea path: spawn progression can't be shown");
                return;
            }

            StackCounts.Clear();
            List<SpawnPoint> spawnPoints = area.SpawnPoints;

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                SpawnPoint spawnPoint = spawnPoints[i];

                // Pilgrims are there from the start: their progression is not used
                if (spawnPoint.Npc is PilgrimData)
                    continue;

                Vector3 marker = path.EvaluatePosition(spawnPoint.Progression);

                using (new Handles.DrawingScope(MarkerColor))
                {
                    Handles.DrawWireDisc(marker, Vector3.up, MARKER_RADIUS);
                    Handles.DrawSolidDisc(marker, Vector3.up, MARKER_RADIUS * 0.4f);
                }

                if (runtimeArea.TryGetAnchor(spawnPoint.AnchorId, out Transform anchor))
                {
                    using (new Handles.DrawingScope(LinkColor))
                        Handles.DrawDottedLine(marker, anchor.position, DASH_SIZE);
                }

                StackCounts.TryGetValue(spawnPoint.Progression, out int stack);
                StackCounts[spawnPoint.Progression] = stack + 1;

                string npcName = spawnPoint.Npc != null ? spawnPoint.Npc.name : "(no NPC)";
                Vector3 labelPosition = marker + Vector3.up * (LABEL_HEIGHT + stack * LABEL_SPACING);
                Handles.Label(labelPosition, $"{spawnPoint.AnchorId} · {npcName} · {spawnPoint.Progression:P0}");
            }
        }
    }
}
