using System;
using System.Collections.Generic;
using System.Linq;
using PFE.Core.Scripts.ComponentSystem;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor.DatabaseBrowser
{
    /// <summary>
    /// Vue réutilisable : liste (avec recherche) des assets d'un type ScriptableObject donné,
    /// trouvés sous Assets/_Project/Resources/Database, plus l'inspecteur de l'asset sélectionné.
    /// Si les assets implémentent IComponentEditor, une barre de familles (filtre exclusif) apparaît
    /// en haut de la fenêtre ; sinon la liste reste plate (ex: BossData, qui n'a pas de famille).
    /// Un bouton de création optionnel peut être fourni par l'outil appelant.
    /// </summary>
    public sealed class DatabaseBrowserView<T> where T : ScriptableObject
    {
        private const string SearchRoot = "Assets/_Project/Resources/Database";
        private static readonly Color DefaultRowColor = new(0.55f, 0.55f, 0.58f);

        private readonly string title;
        private readonly Action onCreateRequested;
        private readonly VisualElement root = new();
        private readonly List<T> assets = new();
        private readonly Dictionary<T, VisualElement> rowsByAsset = new();
        private readonly List<T> visibleAssets = new();
        private readonly Dictionary<string, VisualElement> swatchesByFamily = new();
        private readonly Dictionary<string, VisualElement> familyItemsByFamily = new();
        private readonly Dictionary<string, Color> familyColors = new();

        private VisualElement familyBar;
        private ScrollView listScroll;
        private ScrollView inspectorScroll;
        private UnityEditor.Editor cachedEditor;
        private string searchFilter = string.Empty;
        private string activeFamily;
        private T selected;

        public DatabaseBrowserView(string title, Action onCreateRequested = null)
        {
            this.title = title;
            this.onCreateRequested = onCreateRequested;
        }

        public VisualElement Build()
        {
            root.AddToClassList("db-browser");

            familyBar = new VisualElement();
            familyBar.AddToClassList("db-browser__family-bar");
            root.Add(familyBar);

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

            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { SearchRoot });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            BuildFamilyBar();
            BuildList();
            ReconcileSelection();
        }

        private void BuildFamilyBar()
        {
            familyBar.Clear();
            swatchesByFamily.Clear();
            familyItemsByFamily.Clear();
            familyColors.Clear();

            var families = assets
                .Select(a => a as IComponentEditor)
                .Where(e => e != null)
                .GroupBy(e => e.EditorName)
                .Select(g => (Name: g.Key, Color: g.First().EditorColor))
                .OrderBy(f => f.Name, StringComparer.Ordinal)
                .ToList();

            familyBar.style.display = families.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;

            if (families.Count == 0)
            {
                activeFamily = null;
                return;
            }

            if (activeFamily == null || families.All(f => f.Name != activeFamily))
                activeFamily = families[0].Name;

            for (int i = 0; i < families.Count; i++)
            {
                var family = families[i];

                VisualElement item = new();
                item.AddToClassList("db-browser__family-item");

                VisualElement swatch = new();
                swatch.AddToClassList("db-browser__family-swatch");
                swatch.style.borderTopColor = family.Color;
                swatch.style.borderBottomColor = family.Color;
                swatch.style.borderLeftColor = family.Color;
                swatch.style.borderRightColor = family.Color;

                Label label = new(family.Name);
                label.AddToClassList("db-browser__family-label");

                item.Add(swatch);
                item.Add(label);
                item.RegisterCallback<ClickEvent>(_ => SelectFamily(family.Name));

                swatchesByFamily[family.Name] = swatch;
                familyItemsByFamily[family.Name] = item;
                familyColors[family.Name] = family.Color;
                familyBar.Add(item);

                if (i < families.Count - 1)
                {
                    VisualElement divider = new();
                    divider.AddToClassList("db-browser__family-divider");
                    familyBar.Add(divider);
                }
            }

            RefreshFamilyBarHighlight();
        }

        private void RefreshFamilyBarHighlight()
        {
            foreach (var kvp in swatchesByFamily)
            {
                bool isActive = kvp.Key == activeFamily;
                kvp.Value.style.backgroundColor = isActive ? familyColors[kvp.Key] : new StyleColor(StyleKeyword.Null);
            }

            foreach (var kvp in familyItemsByFamily)
                kvp.Value.EnableInClassList("db-browser__family-item--active", kvp.Key == activeFamily);
        }

        private void SelectFamily(string familyName)
        {
            if (activeFamily == familyName)
                return;

            activeFamily = familyName;
            RefreshFamilyBarHighlight();
            BuildList();
            ReconcileSelection();
        }

        private void BuildList()
        {
            listScroll.Clear();
            rowsByAsset.Clear();
            visibleAssets.Clear();

            IEnumerable<T> visible = assets;

            if (activeFamily != null)
                visible = visible.Where(a => (a as IComponentEditor)?.EditorName == activeFamily);

            if (!string.IsNullOrEmpty(searchFilter))
                visible = visible.Where(a => a.name.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0);

            visibleAssets.AddRange(visible.OrderBy(a => a.name, StringComparer.Ordinal));

            foreach (T asset in visibleAssets)
            {
                Color color = (asset as IComponentEditor)?.EditorColor ?? DefaultRowColor;
                listScroll.Add(BuildRow(asset, color));
            }
        }

        /// <summary>
        /// Si la sélection courante n'est plus visible (changement de famille, recherche, asset
        /// supprimé), sélectionne automatiquement le premier élément de la liste visible.
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

            UnityEditor.Editor.CreateCachedEditor(selected, null, ref cachedEditor);
            InspectorElement inspector = new(cachedEditor);
            inspectorScroll.Add(inspector);
        }
    }
}
