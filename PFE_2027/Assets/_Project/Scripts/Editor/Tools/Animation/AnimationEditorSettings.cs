using UnityEditor;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// Réglages partagés de l'outil d'animation : le template de projet UMotion copié à chaque nouveau
    /// profil, et l'environnement de test instancié dans la stage (généré par défaut si vide).
    /// </summary>
    public sealed class AnimationEditorSettings : ScriptableObject
    {
        private const string SettingsPath = "Assets/_Project/Settings/AnimationEditorSettings.asset";

        [SerializeField, Tooltip("Empty UMotion project copied for every new animation profile.")]
        private Object umotionTemplate;

        [SerializeField, Tooltip("Scenery instantiated in the animation stage (light, ground...). A default one is generated when empty.")]
        private GameObject environmentPrefab;

        public Object UMotionTemplate
        {
            get => umotionTemplate;
            set { umotionTemplate = value; Save(); }
        }

        public GameObject EnvironmentPrefab
        {
            get => environmentPrefab;
            set { environmentPrefab = value; Save(); }
        }

        private static AnimationEditorSettings cached;

        public static AnimationEditorSettings GetOrCreate()
        {
            if (cached != null)
                return cached;

            cached = AssetDatabase.LoadAssetAtPath<AnimationEditorSettings>(SettingsPath);
            if (cached != null)
                return cached;

            cached = CreateInstance<AnimationEditorSettings>();
            EnsureFolder(System.IO.Path.GetDirectoryName(SettingsPath)?.Replace('\\', '/'));
            AssetDatabase.CreateAsset(cached, SettingsPath);
            AssetDatabase.SaveAssets();
            return cached;
        }

        private void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        /// <summary>Crée récursivement un dossier d'assets (ex: "Assets/_Project/Animation/Dummy").</summary>
        internal static void EnsureFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
                return;

            int slash = folder.LastIndexOf('/');
            string parent = folder.Substring(0, slash);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder.Substring(slash + 1));
        }
    }
}
