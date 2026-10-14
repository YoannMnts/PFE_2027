using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Npcs
{
    // Npc tab: one sub-tab per NPC family (Enemy, Pilgrim), each listing its data assets. "+ New" scaffolds a whole
    // new NPC type: its data class, its behaviour class (mapper) and its data asset.
    public sealed class NpcTool : IEditorTool
    {
        private const string BROWSER_STYLE_PATH = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";
        private const string STYLE_PATH = "Assets/_Project/Scripts/Editor/Tools/Npcs/NpcTool.uss";
        private const string SELECTED_CATEGORY_SESSION_KEY = "PFE.Editor.Npc.SelectedCategory";

        public string DisplayName => "Npc";
        public string Icon => "♛";
        public int Order => 10;

        // One sub-tab per NPC family: add a line here for a new family
        private readonly NpcCategory[] categories =
        {
            new EnemyCategory(),
            new PilgrimCategory(),
        };

        private readonly VisualElement root = new();
        private readonly List<Button> categoryButtons = new();
        private readonly List<VisualElement> categoryContents = new();
        private int selectedCategory;

        // Creation panel
        private VisualElement createPanel;
        private Label createTitle;
        private TextField nameField;
        private Label createPreview;
        private Label createError;

        public VisualElement BuildUI()
        {
            root.AddToClassList("npc-tool");
            StyleSheetLoader.Load(root, BROWSER_STYLE_PATH);
            StyleSheetLoader.Load(root, STYLE_PATH);

            VisualElement bar = new();
            bar.AddToClassList("npc-tabs");
            root.Add(bar);

            root.Add(BuildCreatePanel());

            for (int i = 0; i < categories.Length; i++)
            {
                int index = i;
                Button button = new(() => SelectCategory(index)) { text = categories[i].Title };
                button.AddToClassList("npc-tabs__tab");
                bar.Add(button);
                categoryButtons.Add(button);

                VisualElement content = categories[i].Build(ShowCreatePanel);
                content.AddToClassList("npc-category");
                root.Add(content);
                categoryContents.Add(content);
            }

            // The content is detached / reattached when switching tabs: subscribe only while visible
            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                NpcScaffolder.OnAssetCreated -= OnAssetCreated;
                NpcScaffolder.OnAssetCreated += OnAssetCreated;
            });
            root.RegisterCallback<DetachFromPanelEvent>(_ => NpcScaffolder.OnAssetCreated -= OnAssetCreated);

            int saved = SessionState.GetInt(SELECTED_CATEGORY_SESSION_KEY, 0);
            SelectCategory(Mathf.Clamp(saved, 0, categories.Length - 1));
            return root;
        }

        public void OnActivated() => categories[selectedCategory].Reload();
        public void OnDeactivated() { }

        // ---------------------------------------------------------------- sub-tabs

        private void SelectCategory(int index)
        {
            selectedCategory = index;
            SessionState.SetInt(SELECTED_CATEGORY_SESSION_KEY, index);

            for (int i = 0; i < categories.Length; i++)
            {
                bool isSelected = i == index;
                categoryButtons[i].EnableInClassList("npc-tabs__tab--active", isSelected);
                categoryContents[i].style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
            }

            HideCreatePanel();
            categories[index].Reload();
        }

        // ---------------------------------------------------------------- creation

        private VisualElement BuildCreatePanel()
        {
            createPanel = new VisualElement();
            createPanel.AddToClassList("npc-create");
            createPanel.style.display = DisplayStyle.None;

            createTitle = new Label();
            createTitle.AddToClassList("npc-create__title");
            createPanel.Add(createTitle);

            nameField = new TextField("Name");
            nameField.RegisterValueChangedCallback(_ => RefreshPreview());
            createPanel.Add(nameField);

            createPreview = new Label();
            createPreview.AddToClassList("npc-create__preview");
            createPanel.Add(createPreview);

            createError = new Label();
            createError.AddToClassList("npc-create__error");
            createPanel.Add(createError);

            VisualElement buttons = new();
            buttons.AddToClassList("npc-create__buttons");

            Button cancel = new(HideCreatePanel) { text = "Cancel" };
            cancel.AddToClassList("npc-create__secondary-button");
            Button create = new(Create) { text = "Create" };
            create.AddToClassList("npc-create__primary-button");

            buttons.Add(cancel);
            buttons.Add(create);
            createPanel.Add(buttons);

            return createPanel;
        }

        private void ShowCreatePanel()
        {
            createTitle.text = $"New {categories[selectedCategory].Title}";
            nameField.SetValueWithoutNotify(string.Empty);
            createError.text = string.Empty;
            RefreshPreview();

            createPanel.style.display = DisplayStyle.Flex;
            nameField.Focus();
        }

        private void HideCreatePanel()
        {
            if (createPanel != null)
                createPanel.style.display = DisplayStyle.None;
        }

        // Lists the files that will be generated, so the user knows where to find them
        private void RefreshPreview()
        {
            NpcCategory category = categories[selectedCategory];
            string typeName = string.IsNullOrWhiteSpace(nameField.value) ? "<Name>" : nameField.value.Trim();

            createPreview.text = $"{NpcScaffolder.GetDataScriptPath(category, typeName)}\n"
                               + $"{NpcScaffolder.GetBehaviourScriptPath(category, typeName)}\n"
                               + $"{NpcScaffolder.GetAssetPath(category, typeName)}";
        }

        private void Create()
        {
            string error = NpcScaffolder.Create(categories[selectedCategory], nameField.value?.Trim());
            if (error != null)
            {
                createError.text = error;
                return;
            }

            HideCreatePanel();
        }

        // After the domain reload: shows the new asset in its sub-tab
        private void OnAssetCreated(ScriptableObject asset)
        {
            for (int i = 0; i < categories.Length; i++)
            {
                if (!categories[i].Owns(asset))
                    continue;

                SelectCategory(i);
                categories[i].Select(asset);
                return;
            }
        }
    }
}
