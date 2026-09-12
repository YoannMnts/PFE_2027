using System;
using System.IO;
using System.Linq;
using PFE.Core.Scripts.ComponentSystem;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PFE.Editor.ComponentCreation
{
    /// <summary>
    /// Génère un nouveau ComponentData en deux temps : le script doit être compilé avant que
    /// l'asset de son type puisse exister. BeginCreate écrit le .cs et déclenche une recompilation ;
    /// à la fin de celle-ci (DidReloadScripts), le type est retrouvé via TypeCache et l'asset créé.
    /// </summary>
    public static class ComponentGenerator
    {
        private const string PendingNameKey = "PFE.Editor.ComponentGenerator.PendingName";
        private const string PendingFolderKey = "PFE.Editor.ComponentGenerator.PendingFolder";
        private const string ScriptsRoot = "Assets/_Project/Scripts/Core/ComponentSystem";
        private const string AssetsRoot = "Assets/_Project/Resources/Database/Components";

        public static bool BeginCreate(string name, FamilyInfo family, out string error)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(name) || !name.All(char.IsLetterOrDigit) || char.IsDigit(name[0]))
            {
                error = "Name must be a valid C# identifier (letters/digits, cannot start with a digit).";
                return false;
            }

            string className = $"{name}Data";
            string scriptFolder = $"{ScriptsRoot}/{family.FolderName}";
            string scriptPath = $"{scriptFolder}/{className}.cs";

            if (File.Exists(scriptPath))
            {
                error = $"File '{scriptPath}' already exists.";
                return false;
            }

            Directory.CreateDirectory(scriptFolder);

            string content =
                "using UnityEngine;\n\n" +
                "namespace PFE.Core.Scripts.ComponentSystem\n" +
                "{\n" +
                $"    [CreateAssetMenu(menuName = \"PFE/ComponentSystem/{family.FolderName}\", fileName = \"{className}\")]\n" +
                $"    public class {className} : {family.FamilyType.Name}\n" +
                "    {\n" +
                "    }\n" +
                "}\n";

            File.WriteAllText(scriptPath, content);

            SessionState.SetString(PendingNameKey, className);
            SessionState.SetString(PendingFolderKey, family.FolderName);

            AssetDatabase.Refresh();

            error = null;
            return true;
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (string.IsNullOrEmpty(SessionState.GetString(PendingNameKey, string.Empty)))
                return;

            EditorApplication.delayCall += FinishPendingCreation;
        }

        private static void FinishPendingCreation()
        {
            string className = SessionState.GetString(PendingNameKey, string.Empty);
            string folderName = SessionState.GetString(PendingFolderKey, string.Empty);
            SessionState.EraseString(PendingNameKey);
            SessionState.EraseString(PendingFolderKey);

            if (string.IsNullOrEmpty(className))
                return;

            Type dataType = TypeCache.GetTypesDerivedFrom<ComponentData>()
                .FirstOrDefault(t => !t.IsAbstract && t.Name == className);

            if (dataType == null)
            {
                Debug.LogError($"[PFE.Editor] Compilation failed or type '{className}' not found after reload.");
                return;
            }

            string folder = $"{AssetsRoot}/{folderName}";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();

            ScriptableObject asset = ScriptableObject.CreateInstance(dataType);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{className}.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            UnityEngine.Object created = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(created);
            Debug.Log($"[PFE.Editor] Component '{className}' created: {path}");
        }
    }
}
