using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// Isolated animation scene, in the spirit of Prefab mode (and Antros' CutsceneStage): an in-memory
    /// scene (no .unity file) with a test environment + the profile's character, automatically
    /// assigned to UMotion. Nothing is saved there: UMotion handles its own project.
    /// </summary>
    public sealed class AnimationStage : PreviewSceneStage
    {
        [SerializeField] private AnimationProfile profile;
        [SerializeField] private GameObject characterInstance; // survives domain reload

        public static AnimationStage Current => StageUtility.GetCurrentStage() as AnimationStage;
        public AnimationProfile Profile => profile;

        public static void Open(AnimationProfile profile)
        {
            if (profile == null || profile.Character == null)
            {
                Debug.LogWarning("[AnimationStage] The profile has no character prefab to animate.");
                return;
            }

            AnimationStage stage = CreateInstance<AnimationStage>();
            stage.profile = profile;
            StageUtility.GoToStage(stage, true);
        }

        protected override bool OnOpenStage()
        {
            if (!base.OnOpenStage())
                return false;

            BuildEnvironment();

            characterInstance = (GameObject)PrefabUtility.InstantiatePrefab(profile.Character, scene);
            characterInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            ReconnectUMotion();
            EditorApplication.delayCall += FrameCharacter;
            return true;
        }

        protected override void OnCloseStage()
        {
            // Before the scene is destroyed: UMotion must not keep a reference to the character.
            UMotionBridge.Detach();
            base.OnCloseStage();
        }

        protected override GUIContent CreateHeaderContent()
            => new(profile != null ? $"Animation: {profile.name}" : "Animation",
                EditorGUIUtility.IconContent("AnimationClip Icon").image);

        /// <summary>(Re)loads the profile's UMotion project and assigns the stage character to it.</summary>
        public void ReconnectUMotion()
        {
            if (characterInstance == null)
                return;

            string projectPath = profile.UMotionProject != null ? AssetDatabase.GetAssetPath(profile.UMotionProject) : null;
            if (string.IsNullOrEmpty(projectPath))
            {
                Debug.LogWarning($"[AnimationStage] '{profile.name}' has no UMotion project, UMotion was not opened.");
                return;
            }

            UMotionBridge.Attach(projectPath, characterInstance);
        }

        private void BuildEnvironment()
        {
            GameObject environmentPrefab = AnimationEditorSettings.GetOrCreate().EnvironmentPrefab;
            if (environmentPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(environmentPrefab, scene);
                return;
            }

            // Default environment: just enough to see the character.
            GameObject light = new("Directional Light");
            SceneManager.MoveGameObjectToScene(light, scene);
            Light lightComponent = light.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            SceneManager.MoveGameObjectToScene(ground, scene);
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
        }

        private void FrameCharacter()
        {
            if (Current != this || characterInstance == null)
                return;

            Selection.activeGameObject = characterInstance;
            SceneView.lastActiveSceneView?.FrameSelected();
        }
    }
}
