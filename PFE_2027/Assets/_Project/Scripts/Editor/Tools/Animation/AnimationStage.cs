using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// Scène d'animation isolée, dans l'esprit du mode Prefab (et de la CutsceneStage d'Antros) : une scène
    /// en mémoire (aucun .unity) avec un environnement de test + le personnage du profil, assigné
    /// automatiquement à UMotion. Rien n'y est sauvegardé : UMotion gère son propre projet.
    /// </summary>
    public sealed class AnimationStage : PreviewSceneStage
    {
        [SerializeField] private AnimationProfile profile;
        [SerializeField] private GameObject characterInstance; // survit au domain reload

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
            // Avant la destruction de la scène : UMotion ne doit pas garder une référence vers le perso.
            UMotionBridge.Detach();
            base.OnCloseStage();
        }

        protected override GUIContent CreateHeaderContent()
            => new(profile != null ? $"Animation: {profile.name}" : "Animation",
                EditorGUIUtility.IconContent("AnimationClip Icon").image);

        /// <summary>(Re)charge le projet UMotion du profil et lui assigne le personnage de la stage.</summary>
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

            // Environnement par défaut : juste de quoi voir le perso.
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
