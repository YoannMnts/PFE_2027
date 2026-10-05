using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.DatabaseBrowser
{
    /// <summary>
    /// Reusable view: searchable list of the assets of a given ScriptableObject type,
    /// found under Assets/_Project/Resources/Database (or another provided folder), plus the inspector of the selected asset.
    /// An optional create button can be provided by the calling tool, as well as an optional actions bar
    /// displayed above the inspector of the selected asset.
    /// </summary>
    public sealed class DatabaseBrowserView<T> where T : ScriptableObject
    {
        private const string DefaultSearchRoot = "Assets/_Project/Resources/Database";
        private static readonly Color DefaultRowColor = new(0.55f, 0.55f, 0.58f);

        private readonly string title;
        private readonly string searchRoot;
        private readonly Action onCreateRequested;
        private readonly Action<T, VisualElement> buildSelectionActions;
        private readonly VisualElement root = new();
        private readonly List<T> assets = new();
        private readonly Dictionary<T, VisualElement> rowsByAsset = new();
        private readonly List<T> visibleAssets = new();

        private ScrollView listScroll;
        private ScrollView inspectorScroll;
        private UnityEditor.Editor cachedEditor;
        private string searchFilter = string.Empty;
        private T selected;

        public DatabaseBrowserView(string title, Action onCreateRequested = null,
            Action<T, VisualElement> buildSelectionActions = null, string searchRoot = null)
        {
            this.title = title;
            this.searchRoot = string.IsNullOrEmpty(searchRoot) ? DefaultSearchRoot : searchRoot;
            this.onCreateRequested = onCreateRequested;
            this.buildSelectionActions = buildSelectionActions;
        }

        public VisualElement Build()
        {
            root.AddToClassList("db-browser");

            VisualElement body = new();
            body.AddToClassList("db-browser__body");

            VisualElement listPane = new();
            listPane.AddToClassList("db-browser__list-pane");

            if (onCreateRequested != null)
            {
                Button createButton = new(() => onCreateRequested())
                {
                    text = $"+ New {title}",
                };
                createButton.AddToClassList("db-browser__create-button");
                listPane.Add(createButton);
            }

            ToolbarSearchField search = new();
            search.AddToClassList("db-browser__search");
            search.RegisterValueChangedCallback(evt =>
            {
                searchFilter = evt.newValue ?? string.Empty;
                Reload();
            });
            listPane.Add(search);

            listScroll = new ScrollView(ScrollViewMode.Vertical);
            listScroll.AddToClassList("db-browser__list");
            listPane.Add(listScroll);

            inspectorScroll = new ScrollView(ScrollViewMode.Vertical);
            inspectorScroll.AddToClassList("db-browser__inspector");

            body.Add(listPane);
            body.Add(inspectorScroll);
            root.Add(body);

            Reload();
            return root;
        }

        public void Reload()
        {
            assets.Clear();

            // Folder not created yet (e.g. no animation profile): empty list rather than an error.
            if (!AssetDatabase.IsValidFolder(searchRoot))
            {
                BuildList();
                ReconcileSelection();
                return;
            }

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            BuildList();
            ReconcileSelection();
        }

        private void BuildList()
        {
            listScroll.Clear();
            rowsByAsset.Clear();
            visibleAssets.Clear();

            IEnumerable<T> visible = assets;

            if (!string.IsNullOrEmpty(searchFilter))
                visible = visible.Where(a => a.name.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            visibleAssets.AddRange(visible.OrderBy(a => a.name, StringComparer.Ordinal));

            foreach (T asset in visibleAssets)
                listScroll.Add(BuildRow(asset, DefaultRowColor));
        }

        /// <summary>
        /// Selects an asset from outside (e.g. when a tool leaves its edit mode).
        /// </summary>
        public void SetSelection(T asset)
        {
            selected = asset;
            ReconcileSelection();
        }

        /// <summary>
        /// If the current selection is no longer visible (search, deleted asset), automatically
        /// selects the first item of the visible list.
        /// </summary>
        private void ReconcileSelection()
        {
            T target = selected != null && visibleAssets.Contains(selected) ? selected : visibleAssets.FirstOrDefault();
            Select(target);
        }

        private VisualElement BuildRow(T asset, Color color)
        {
            VisualElement row = new();
            row.AddToClassList("db-browser__row");
            row.EnableInClassList("db-browser__row--selected", asset == selected);

            VisualElement dot = new();
            dot.AddToClassList("db-browser__row-dot");
            dot.style.backgroundColor = color;

            Label label = new(asset != null ? asset.name : "(missing)");
            label.AddToClassList("db-browser__row-label");

            row.Add(dot);
            row.Add(label);
            row.RegisterCallback<ClickEvent>(_ => Select(asset));

            rowsByAsset[asset] = row;

            return row;
        }

        private void Select(T asset)
        {
            selected = asset;

            foreach (var kvp in rowsByAsset)
                kvp.Value.EnableInClassList("db-browser__row--selected", kvp.Key == selected);

            ShowSelected();
        }

        private void ShowSelected()
        {
            inspectorScroll.Clear();

            if (selected == null)
            {
                inspectorScroll.Add(new Label($"Select a {title} from the list."));
                return;
            }

            if (buildSelectionActions != null)
            {
                VisualElement actions = new();
                actions.AddToClassList("db-browser__selection-actions");
                buildSelectionActions(selected, actions);
                inspectorScroll.Add(actions);
            }

            UnityEditor.Editor.CreateCachedEditor(selected, null, ref cachedEditor);
            InspectorElement inspector = new(cachedEditor);
            inspectorScroll.Add(inspector);
        }
    }
}
