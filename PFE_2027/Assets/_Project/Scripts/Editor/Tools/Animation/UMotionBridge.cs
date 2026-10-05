using System;
using UMotionEditor.API;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// ONLY point of contact with the UMotion API. UMotion windows initialize over several frames,
    /// so the assignment happens in several steps driven by EditorApplication.update:
    /// open the windows → wait for their init → load the project → assign the character.
    /// </summary>
    internal static class UMotionBridge
    {
        private const double TimeoutSeconds = 10.0;
        private const double RefocusDelaySeconds = 1.0;

        private static string pendingProjectPath;
        private static GameObject pendingTarget;
        private static double startTime;
        private static bool projectRequested;
        private static bool refocused;

        public static void Attach(string projectPath, GameObject target)
        {
            Cancel();

            if (string.IsNullOrEmpty(projectPath) || target == null)
            {
                Debug.LogWarning("[UMotionBridge] Missing UMotion project or target, nothing to attach.");
                return;
            }

            pendingProjectPath = Normalize(projectPath);
            pendingTarget = target;
            startTime = EditorApplication.timeSinceStartup;
            projectRequested = false;
            refocused = false;

            try
            {
                ClipEditor.OpenWindow();
                PoseEditor.OpenWindow();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UMotionBridge] Could not open the UMotion windows: {e.Message}");
                Cancel();
                return;
            }

            EditorApplication.update += Tick;
        }

        /// <summary>Removes the character from UMotion (call before the preview scene goes away).</summary>
        public static void Detach()
        {
            Cancel();

            try
            {
                if (PoseEditor.IsWindowOpened && PoseEditor.AnimatedPreviewGameObject != null)
                    PoseEditor.ClearAnimatedGameObject(PoseEditor.ClearMode.RevertChanges);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[UMotionBridge] Could not clear the UMotion animated object: {e.Message}");
            }
        }

        private static void Cancel()
        {
            EditorApplication.update -= Tick;
            pendingProjectPath = null;
            pendingTarget = null;
        }

        private static void Tick()
        {
            // Target destroyed in the meantime (stage closed): give up silently.
            if (pendingTarget == null)
            {
                Cancel();
                return;
            }

            double elapsed = EditorApplication.timeSinceStartup - startTime;
            if (elapsed > TimeoutSeconds)
            {
                Debug.LogWarning("[UMotionBridge] UMotion did not finish initializing in time. " +
                                 "Focus the Clip Editor or Pose Editor window, then use 'Reconnect UMotion'.");
                Cancel();
                return;
            }

            try
            {
                if (!ClipEditor.IsWindowOpened || !PoseEditor.IsWindowOpened)
                {
                    // According to the UMotion manual, a window may need focus to initialize.
                    if (!refocused && elapsed > RefocusDelaySeconds)
                    {
                        refocused = true;
                        ClipEditor.OpenWindow();
                        PoseEditor.OpenWindow();
                    }
                    return;
                }

                if (!string.Equals(Normalize(ClipEditor.GetLoadedProjectPath()), pendingProjectPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (!projectRequested)
                    {
                        projectRequested = true;
                        ClipEditor.LoadProject(pendingProjectPath);
                    }
                    return;
                }

                if (PoseEditor.AnimatedPreviewGameObject != null)
                    PoseEditor.ClearAnimatedGameObject(PoseEditor.ClearMode.RevertChanges);

                PoseEditor.SetAnimatedGameObject(pendingTarget);
                MoveDuplicateToTargetScene(pendingTarget);
                Debug.Log($"[UMotionBridge] '{pendingTarget.name}' assigned to UMotion project '{pendingProjectPath}'.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[UMotionBridge] UMotion setup failed: {e.Message}");
            }

            Cancel();
        }

        // UMotion animates a COPY of the object (the original is hidden) and creates it in the active scene,
        // so in the main scene rather than in the stage's in-memory scene: invisible in
        // the SceneView, only the bone gizmos remain. We move it back next to the original.
        private static void MoveDuplicateToTargetScene(GameObject target)
        {
            GameObject duplicate = PoseEditor.AnimatedPreviewGameObject;
            if (duplicate == null || duplicate.scene == target.scene)
                return;

            if (duplicate.transform.parent != null)
            {
                Debug.LogWarning($"[UMotionBridge] UMotion's preview copy of '{target.name}' is not a root object and " +
                                 "could not be moved into the stage scene.");
                return;
            }

            SceneManager.MoveGameObjectToScene(duplicate, target.scene);
        }

        private static string Normalize(string path) => path?.Replace('\\', '/');
    }
}
