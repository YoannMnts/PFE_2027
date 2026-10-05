using System.Collections.Generic;
using PFE.Core.Scripts.Area;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools
{
    /// <summary>
    /// Area tab: list of AreaData (like the Enemy tab) + "Edit Prefab" button on the selected area.
    /// When an area prefab is open (through the button or any other way), the tab switches to edit mode
    /// and shows the data inspector: SpawnAnchors are placed in the SceneView and enemies are assigned here.
    /// </summary>
    public sealed class AreaBrowserTool : IEditorTool
    {
        private const string BrowserStylePath = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";
        private const string AreaStylePath = "Assets/_Project/Scripts/Editor/Tools/AreaBrowserTool.uss";
        private const string SearchRoot = "Assets/_Project/Resources/Database";

        public string DisplayName => "Area";
        public string Icon => "▦";
        public int Order => 20;

        private readonly VisualElement root = new();
        private readonly List<AreaData> editableAreas = new();

        private DatabaseBrowserView<AreaData> browser;
        private VisualElement browserContent;
        private VisualElement editPanel;
        private UnityEditor.Editor cachedEditor;

        // Area picked through "Edit Prefab": takes priority when several datas share the same prefab.
        private AreaData requestedArea;
        private AreaData editedArea;

        public VisualElement BuildUI()
        {
            root.AddToClassList("area-tool");
            StyleSheetLoader.Load(root, BrowserStylePath);
            StyleSheetLoader.Load(root, AreaStylePath);

            browser = new DatabaseBrowserView<AreaData>(DisplayName, buildSelectionActions: BuildSelectionActions);
            browserContent = browser.Build();
            root.Add(browserContent);

            editPanel = new VisualElement();
            editPanel.AddToClassList("area-edit");
            root.Add(editPanel);

            // Prefab stages are only listened to while the tab is displayed (the content is detached
            // when switching tab and when closing the window). OnActivated resyncs on return.
            root.RegisterCallback<AttachToPanelEvent>(_ => Subscribe());
            root.RegisterCallback<DetachFromPanelEvent>(_ => Unsubscribe());

            ShowBrowser();
            return root;
        }

        public void OnActivated()
        {
            browser?.Reload();
            RefreshMode(PrefabStageUtility.GetCurrentPrefabStage());
        }

        public void OnDeactivated() { }

        private void Subscribe()
        {
            Unsubscribe();
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            PrefabStage.prefabStageClosing += OnPrefabStageClosing;
        }

        private void Unsubscribe()
        {
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageClosing -= OnPrefabStageClosing;
        }

        private void OnPrefabStageOpened(PrefabStage stage) => RefreshMode(stage);

        // During "closing", the stage is still the current stage: wait for the next frame to know
        // where we return to (main scene, or parent prefab when editing a nested prefab).
        private void OnPrefabStageClosing(PrefabStage stage)
            => root.schedule.Execute(() => RefreshMode(PrefabStageUtility.GetCurrentPrefabStage()));

        private void BuildSelectionActions(AreaData area, VisualElement container)
        {
            Button editButton = new(() => OpenPrefab(area)) { text = "Edit Prefab" };
            editButton.AddToClassList("area-tool__edit-button");
            container.Add(editButton);
        }

        private void OpenPrefab(AreaData area)
        {
            if (area.Prefab == null)
            {
                Debug.LogWarning($"[AreaBrowserTool] '{area.name}' has no Prefab assigned.", area);
                return;
            }

            requestedArea = area;
            string path = AssetDatabase.GetAssetPath(area.Prefab);

            PrefabStage current = PrefabStageUtility.GetCurrentPrefabStage();
            if (current != null && current.assetPath == path)
            {
                RefreshMode(current);
                return;
            }

            PrefabStageUtility.OpenPrefab(path);
        }

        private void RefreshMode(PrefabStage stage)
        {
            editableAreas.Clear();
            if (stage != null)
                CollectAreasUsingPrefab(stage.assetPath, editableAreas);

            if (editableAreas.Count == 0)
            {
                AreaData lastEdited = editedArea;
                editedArea = null;
                requestedArea = null;
                ShowBrowser();

                if (lastEdited != null)
                    browser.SetSelection(lastEdited);
                return;
            }

            if (requestedArea != null && editableAreas.Contains(requestedArea))
                editedArea = requestedArea;
            else if (editedArea == null || !editableAreas.Contains(editedArea))
                editedArea = editableAreas[0];

            ShowEditPanel();
        }

        private static void CollectAreasUsingPrefab(string prefabPath, List<AreaData> results)
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(AreaData)}", new[] { SearchRoot });
            foreach (string guid in guids)
            {
                AreaData area = AssetDatabase.LoadAssetAtPath<AreaData>(AssetDatabase.GUIDToAssetPath(guid));
                if (area != null && area.Prefab != null && AssetDatabase.GetAssetPath(area.Prefab) == prefabPath)
                    results.Add(area);
            }

            results.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        }

        private void ShowBrowser()
        {
            browserContent.style.display = DisplayStyle.Flex;
            editPanel.style.display = DisplayStyle.None;
        }

        private void ShowEditPanel()
        {
            editPanel.Clear();

            VisualElement header = new();
            header.AddToClassList("area-edit__header");

            VisualElement titles = new();
            titles.AddToClassList("area-edit__titles");

            Label title = new($"Editing: {editedArea.name}");
            title.AddToClassList("area-edit__title");
            Label subtitle = new($"Prefab: {editedArea.Prefab.name}");
            subtitle.AddToClassList("area-edit__subtitle");

            titles.Add(title);
            titles.Add(subtitle);
            header.Add(titles);

            Button backButton = new(() => StageUtility.GoToMainStage()) { text = "Back to Scene" };
            backButton.AddToClassList("area-edit__button");
            header.Add(backButton);

            editPanel.Add(header);

            if (editableAreas.Count > 1)
            {
                List<string> choices = editableAreas.ConvertAll(a => a.name);
                DropdownField dropdown = new("Area Data", choices, editableAreas.IndexOf(editedArea));
                dropdown.AddToClassList("area-edit__dropdown");
                dropdown.RegisterValueChangedCallback(_ =>
                {
                    if (dropdown.index < 0)
                        return;

                    editedArea = editableAreas[dropdown.index];
                    requestedArea = editedArea;
                    // Rebuilds the panel after the callback, not during it.
                    editPanel.schedule.Execute(ShowEditPanel);
                });
                editPanel.Add(dropdown);
            }

            ScrollView inspectorScroll = new(ScrollViewMode.Vertical);
            inspectorScroll.AddToClassList("area-edit__inspector");

            UnityEditor.Editor.CreateCachedEditor(editedArea, null, ref cachedEditor);
            inspectorScroll.Add(new InspectorElement(cachedEditor));
            editPanel.Add(inspectorScroll);

            browserContent.style.display = DisplayStyle.None;
            editPanel.style.display = DisplayStyle.Flex;
        }
    }
}
