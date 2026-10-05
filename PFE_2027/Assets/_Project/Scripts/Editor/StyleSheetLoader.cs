using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor
{
    /// <summary>
    /// Loads a style sheet from its exact asset path
    /// (e.g. "Assets/_Project/Scripts/Editor/PfeEditorWindow.uss").
    /// Named StyleSheetLoader rather than EditorStyles to avoid any ambiguity with UnityEditor.EditorStyles.
    /// </summary>
    public static class StyleSheetLoader
    {
        public static void Load(VisualElement root, string assetPath)
        {
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            if (sheet == null)
            {
                Debug.LogWarning($"[PFE.Editor] Feuille de style introuvable : '{assetPath}'.");
                return;
            }

            root.styleSheets.Add(sheet);
        }
    }
}
