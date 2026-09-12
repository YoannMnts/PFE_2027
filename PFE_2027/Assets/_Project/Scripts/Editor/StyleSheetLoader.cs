using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor
{
    /// <summary>
    /// Charge une feuille de style par son chemin d'asset exact
    /// (ex: "Assets/_Project/Scripts/Editor/PfeEditorWindow.uss").
    /// Nommé StyleSheetLoader plutôt que EditorStyles pour éviter toute ambiguïté avec UnityEditor.EditorStyles.
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
