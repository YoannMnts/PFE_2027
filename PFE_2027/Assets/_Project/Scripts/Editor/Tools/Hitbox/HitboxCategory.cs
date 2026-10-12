using System;
using System.Collections.Generic;
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
        public string Title { get; }

        protected HitboxCategory(string title) => Title = title;

        public abstract VisualElement Build(Action<HitboxData, VisualElement> buildSelectionActions, string searchRoot);
        public abstract void Reload();
    }

    public sealed class HitboxCategory<T> : HitboxCategory where T : HitboxData
    {
        // Folder proposed by the save dialog when creating a new asset
        private readonly string createFolder;
        private DatabaseBrowserView<T> browser;

        public HitboxCategory(string title, string createFolder) : base(title) => this.createFolder = createFolder;

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

        private void Create(Type type)
        {
            string defaultName = $"New {ObjectNames.NicifyVariableName(type.Name)}";
            string path = EditorUtility.SaveFilePanelInProject($"New {Title}", defaultName, "asset",
                $"Choose where to save the new {Title}.", createFolder);

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
