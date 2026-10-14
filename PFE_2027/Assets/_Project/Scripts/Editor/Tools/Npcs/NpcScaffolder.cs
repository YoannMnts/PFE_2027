using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PFE.Core.Scripts.NPCs;
using UnityEditor;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Npcs
{
    // Generates a new NPC type: data script + behaviour script right away, data asset once Unity compiled them.
    // The asset can only be created after the domain reload (its class doesn't exist before), so the request
    // survives the reload in SessionState.
    public static class NpcScaffolder
    {
        private const string PENDING_SESSION_KEY = "PFE.Editor.Npc.PendingAsset";
        private const string GUID_FIELD = "guidText";                    // GameDatabaseObject
        private const string NAME_FIELD = "<Name>k__BackingField";       // NpcData.Name

        private static readonly Regex TypeNameRegex = new("^[A-Z][A-Za-z0-9]*$");
        private static readonly UTF8Encoding Utf8WithBom = new(true);

        // Raised once the data asset of a generated type exists
        public static event Action<ScriptableObject> OnAssetCreated;

        [Serializable]
        private struct PendingAsset
        {
            public string dataTypeName;     // Full name of the generated data class
            public string assetPath;
            public string displayName;
        }

        public static string GetDataScriptPath(NpcCategory category, string typeName) =>
            $"{category.DataScriptFolder}/{typeName}/{typeName}Data.cs";

        public static string GetBehaviourScriptPath(NpcCategory category, string typeName) =>
            $"{category.BehaviourScriptFolder}/{typeName}/{typeName}.cs";

        public static string GetAssetPath(NpcCategory category, string typeName) =>
            $"{category.AssetFolder}/{typeName}Data.asset";

        // Writes the scripts and asks Unity to compile them. Returns an error message, or null on success.
        public static string Create(NpcCategory category, string typeName)
        {
            if (EditorApplication.isCompiling)
                return "Unity is compiling, try again in a moment.";

            if (string.IsNullOrEmpty(typeName) || !TypeNameRegex.IsMatch(typeName))
                return "Use a PascalCase C# name: letters and digits, starting with an uppercase letter.";

            if (typeName.EndsWith("Data", StringComparison.Ordinal))
                return "Don't add the 'Data' suffix, it is added for you.";

            if (TypeExists(typeName) || TypeExists(typeName + "Data"))
                return $"A type named '{typeName}' or '{typeName}Data' already exists.";

            string dataPath = GetDataScriptPath(category, typeName);
            string behaviourPath = GetBehaviourScriptPath(category, typeName);
            string assetPath = GetAssetPath(category, typeName);

            if (File.Exists(dataPath) || File.Exists(behaviourPath))
                return "Scripts with this name already exist.";

            if (File.Exists(assetPath))
                return $"'{assetPath}' already exists.";

            WriteScript(dataPath, category.BuildDataScript(typeName));
            WriteScript(behaviourPath, category.BuildBehaviourScript(typeName));
            Directory.CreateDirectory(category.AssetFolder);

            PendingAsset pending = new()
            {
                dataTypeName = $"{category.DataNamespace}.{typeName}Data",
                assetPath = assetPath,
                displayName = ObjectNames.NicifyVariableName(typeName),
            };
            SessionState.SetString(PENDING_SESSION_KEY, JsonUtility.ToJson(pending));

            // Compiles the new scripts: the asset is created after the domain reload
            AssetDatabase.Refresh();
            return null;
        }

        // Same file format as the rest of the project: UTF-8 with BOM, CRLF
        private static void WriteScript(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string crlf = content.Replace("\r\n", "\n").Replace("\n", "\r\n");
            File.WriteAllText(path, crlf, Utf8WithBom);
        }

        // Project types only (PFE assemblies and Assembly-CSharp, where the Behavior nodes live)
        private static bool TypeExists(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string assemblyName = assembly.GetName().Name;
                if (!assemblyName.StartsWith("PFE", StringComparison.Ordinal) && assemblyName != "Assembly-CSharp")
                    continue;

                if (assembly.GetTypes().Any(t => t.Name == typeName))
                    return true;
            }

            return false;
        }

        // ---------------------------------------------------------------- after the reload

        [InitializeOnLoadMethod]
        private static void OnDomainReloaded()
        {
            if (!string.IsNullOrEmpty(SessionState.GetString(PENDING_SESSION_KEY, string.Empty)))
                EditorApplication.delayCall += CompletePending;
        }

        private static void CompletePending()
        {
            string json = SessionState.GetString(PENDING_SESSION_KEY, string.Empty);

            // Still compiling: the next domain reload will try again
            if (string.IsNullOrEmpty(json) || EditorApplication.isCompiling)
                return;

            PendingAsset pending = JsonUtility.FromJson<PendingAsset>(json);
            Type dataType = TypeCache.GetTypesDerivedFrom<NpcData>().FirstOrDefault(t => t.FullName == pending.dataTypeName);

            // Not compiled yet (compile error elsewhere in the project...): kept until a reload brings it
            if (dataType == null)
                return;

            SessionState.EraseString(PENDING_SESSION_KEY);

            ScriptableObject asset = ScriptableObject.CreateInstance(dataType);
            AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(pending.assetPath));

            // CreateInstance doesn't run OnValidate: database id and display name assigned right away
            SerializedObject serialized = new(asset);
            serialized.FindProperty(GUID_FIELD).stringValue = Guid.NewGuid().ToString();
            SerializedProperty nameProperty = serialized.FindProperty(NAME_FIELD);
            if (nameProperty != null)
                nameProperty.stringValue = pending.displayName;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);
            OnAssetCreated?.Invoke(asset);
        }
    }
}
