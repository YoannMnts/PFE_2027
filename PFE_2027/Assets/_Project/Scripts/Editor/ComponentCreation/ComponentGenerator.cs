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
        private const string ScriptsRoot = "Assets/_Project/Scripts/Core/ComponentDatas";
        private const string GameplayRoot = "Assets/_Project/Scripts/Gameplay/ComponentSystem/Components";
        private const string AssetsRoot = "Assets/_Project/Resources/Database/Components";

        public static bool BeginCreate(string name, FamilyInfo family, out string error)
        {
            name = (name ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(name) || !name.All(char.IsLetterOrDigit) || char.IsDigit(name[0]))
            {
                error = "Name must be a valid C# identifier (letters/digits, cannot start with a digit).";
                return false;
            }

            string dataClassName = $"{name}Data";
            string dataFolder = $"{ScriptsRoot}/{family.FolderName}/{name}";
            string dataScriptPath = $"{dataFolder}/{dataClassName}.cs";

            if (File.Exists(dataScriptPath))
            {
                error = $"File '{dataScriptPath}' already exists.";
                return false;
            }

            string componentClassName = $"{name}Component";
            string componentFolder = $"{GameplayRoot}/{family.FolderName}/{name}";
            string componentScriptPath = $"{componentFolder}/{componentClassName}.cs";

            if (family.ComponentInterfaceType != null && File.Exists(componentScriptPath))
            {
                error = $"File '{componentScriptPath}' already exists.";
                return false;
            }

            Directory.CreateDirectory(dataFolder);

            string dataContent =
                "using UnityEngine;\n\n" +
                "namespace PFE.Core.Scripts.ComponentSystem\n" +
                "{\n" +
                $"    [CreateAssetMenu(menuName = \"PFE/ComponentSystem/{family.FolderName}\", fileName = \"{dataClassName}\")]\n" +
                $"    public class {dataClassName} : {family.FamilyType.Name}\n" +
                "    {\n" +
                "    }\n" +
                "}\n";

            File.WriteAllText(dataScriptPath, dataContent);

            if (family.ComponentInterfaceType != null && !string.IsNullOrEmpty(family.ActionMethodName))
            {
                string interfaceName = family.ComponentInterfaceType.Name.Split('`')[0];

                Directory.CreateDirectory(componentFolder);

                string componentContent =
                    "using PFE.Core.Scripts.ComponentSystem;\n\n" +
                    "namespace PFE.Gameplay.Scripts.ComponentSystem\n" +
                    "{\n" +
                    $"    public partial struct {componentClassName} : {interfaceName}<{dataClassName}>\n" +
                    "    {\n" +
                    $"        public bool CanTrigger({dataClassName} data, ComponentContext context)\n" +
                    "        {\n" +
                    "            return true;\n" +
                    "        }\n\n" +
                    $"        public void {family.ActionMethodName}({dataClassName} data)\n" +
                    "        {\n" +
                    "            // TODO\n" +
                    "        }\n" +
                    "    }\n" +
                    "}\n";

                File.WriteAllText(componentScriptPath, componentContent);
            }
            else
            {
                Debug.LogWarning($"[PFE.Editor] No gameplay component interface found for family '{family.EditorName}' — only the data script was generated.");
            }

            SessionState.SetString(PendingNameKey, dataClassName);
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
