using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Hitbox
{
    // One sub-tab of the Hitbox tab: the list of the assets of one HitboxData type, with an "Edit" button
    // and a "+ New" button that creates an asset of a concrete type deriving from it.
    // Non-generic base so the tool can keep every category in the same list.
    public abstract class HitboxCategory
    {
        protected const string HITBOXES_FOLDER = "Assets/_Project/Resources/Database/Hitboxes";

        public string Title { get; }

        protected HitboxCategory(string title) => Title = title;

        public abstract VisualElement Build(Action<HitboxData, VisualElement> buildSelectionActions, string searchRoot);
        public abstract void Reload();

        // One category per type deriving directly from HitboxData (AttackData, ChargeData, DashData, PushbackData...),
        // sorted by title: a new branch of HitboxData gets its tab without touching the tool.
        // Deeper types (BasicAttackData : AttackData) are listed and created in the tab of their branch.
        public static HitboxCategory[] CreateForEveryType()
        {
            return TypeCache.GetTypesDerivedFrom<HitboxData>()
                .Where(type => type.BaseType == typeof(HitboxData) && !type.IsGenericTypeDefinition)
                .Select(type => (HitboxCategory)Activator.CreateInstance(typeof(HitboxCategory<>).MakeGenericType(type), GetTitle(type)))
                .OrderBy(category => category.Title, StringComparer.Ordinal)
                .ToArray();
        }

        // "AttackData" -> "Attack", "ChargeData" -> "Charge"
        private static string GetTitle(Type type)
        {
            const string SUFFIX = "Data";
            string name = type.Name.EndsWith(SUFFIX, StringComparison.Ordinal) && type.Name.Length > SUFFIX.Length
                ? type.Name[..^SUFFIX.Length]
                : type.Name;

            return ObjectNames.NicifyVariableName(name);
        }
    }

    public sealed class HitboxCategory<T> : HitboxCategory where T : HitboxData
    {
        private DatabaseBrowserView<T> browser;

        // Called through reflection by CreateForEveryType
        public HitboxCategory(string title) : base(title) { }

        public override VisualElement Build(Action<HitboxData, VisualElement> buildSelectionActions, string searchRoot)
        {
            // Action is contravariant: an Action<HitboxData, ...> is a valid Action<T, ...>
            browser = new DatabaseBrowserView<T>(Title, RequestCreate, buildSelectionActions, searchRoot);
            return browser.Build();
        }

        public override void Reload() => browser?.Reload();

        // ---------------------------------------------------------------- creation

        private void RequestCreate()
        {
            List<Type> types = FindConcreteTypes();

            if (types.Count == 0)
            {
                Debug.LogWarning($"[HitboxTool] No concrete type derives from {typeof(T).Name}: create one first.");
                return;
            }

            if (types.Count == 1)
            {
                Create(types[0]);
                return;
            }

            // Several concrete types: let the user pick which one to create
            GenericMenu menu = new();
            foreach (Type type in types)
            {
                Type captured = type;
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () => Create(captured));
            }
            menu.ShowAsContext();
        }

        private static List<Type> FindConcreteTypes()
        {
            IEnumerable<Type> types = TypeCache.GetTypesDerivedFrom<T>().Append(typeof(T));

            return types
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Distinct()
                .OrderBy(t => t.Name, StringComparer.Ordinal)
                .ToList();
        }

        // Folder of the existing assets of this type (first one by path), or the Hitboxes folder itself
        // for a type without any asset yet (the user picks or creates its sub-folder in the dialog)
        private static string GetCreateFolder()
        {
            string firstPath = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { HITBOXES_FOLDER })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .FirstOrDefault();

            return string.IsNullOrEmpty(firstPath)
                ? HITBOXES_FOLDER
                : Path.GetDirectoryName(firstPath)?.Replace('\\', '/') ?? HITBOXES_FOLDER;
        }

        private void Create(Type type)
        {
            string defaultName = $"New {ObjectNames.NicifyVariableName(type.Name)}";
            string path = EditorUtility.SaveFilePanelInProject($"New {Title}", defaultName, "asset",
                $"Choose where to save the new {Title}.", GetCreateFolder());

            // Dialog cancelled
            if (string.IsNullOrEmpty(path))
                return;

            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);

            // CreateInstance does not run OnValidate: assign the database id right away
            SerializedObject serialized = new(asset);
            serialized.FindProperty(HitboxPropertyPaths.GuidText).stringValue = Guid.NewGuid().ToString();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();

            browser.Reload();
            browser.SetSelection((T)asset);
            EditorGUIUtility.PingObject(asset);
        }
    }
}
