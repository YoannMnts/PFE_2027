using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.AnimationConnector
{
    // One sub-tab of the Animation Connector tab: the list of the assets of one AnimationData type, with an "Edit" button
    // and a "+ New" button that creates an asset of a concrete type deriving from it.
    // Non-generic base so the tool can keep every category in the same list.
    public abstract class AnimationConnectorCategory
    {
        protected const string ANIMATIONS_FOLDER = "Assets/_Project/Resources/Database/Animations";

        public string Title { get; }

        protected AnimationConnectorCategory(string title) => Title = title;

        public abstract VisualElement Build(Action<AnimationData, VisualElement> buildSelectionActions, string searchRoot);
        public abstract void Reload();

        // One category per type deriving directly from AnimationData (AttackData, ChargeData, DashData, PushbackData...),
        // sorted by title: a new branch of AnimationData gets its tab without touching the tool.
        // Deeper types (BasicAttackData : AttackData) are listed and created in the tab of their branch.
        public static AnimationConnectorCategory[] CreateForEveryType()
        {
            return TypeCache.GetTypesDerivedFrom<AnimationData>()
                .Where(type => type.BaseType == typeof(AnimationData) && !type.IsGenericTypeDefinition)
                .Select(type => (AnimationConnectorCategory)Activator.CreateInstance(typeof(AnimationConnectorCategory<>).MakeGenericType(type), GetTitle(type)))
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

    public sealed class AnimationConnectorCategory<T> : AnimationConnectorCategory where T : AnimationData
    {
        private DatabaseBrowserView<T> browser;

        // Called through reflection by CreateForEveryType
        public AnimationConnectorCategory(string title) : base(title) { }

        public override VisualElement Build(Action<AnimationData, VisualElement> buildSelectionActions, string searchRoot)
        {
            // Action is contravariant: an Action<AnimationData, ...> is a valid Action<T, ...>
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
                Debug.LogWarning($"[AnimationConnectorTool] No concrete type derives from {typeof(T).Name}: create one first.");
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

        // Folder of the existing assets of this type (first one by path), or the Animations folder itself
        // for a type without any asset yet (the user picks or creates its sub-folder in the dialog)
        private static string GetCreateFolder()
        {
            string firstPath = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { ANIMATIONS_FOLDER })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path, StringComparer.Ordinal)
                .FirstOrDefault();

            return string.IsNullOrEmpty(firstPath)
                ? ANIMATIONS_FOLDER
                : Path.GetDirectoryName(firstPath)?.Replace('\\', '/') ?? ANIMATIONS_FOLDER;
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
            serialized.FindProperty(AnimationConnectorPropertyPaths.GuidText).stringValue = Guid.NewGuid().ToString();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();

            browser.Reload();
            browser.SetSelection((T)asset);
            EditorGUIUtility.PingObject(asset);
        }
    }
}
