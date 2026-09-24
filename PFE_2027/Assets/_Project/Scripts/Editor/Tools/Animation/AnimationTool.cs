using System.IO;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Animation
{
    /// <summary>
    /// Onglet Animation : liste des profils d'animation, création d'un profil (copie du template UMotion)
    /// et ouverture de la stage d'animation qui branche automatiquement le perso dans UMotion.
    /// </summary>
    public sealed class AnimationTool : IEditorTool
    {
        private const string BrowserStylePath = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";
        private const string ToolStylePath = "Assets/_Project/Scripts/Editor/Tools/Animation/AnimationTool.uss";
        private const string ProfilesRoot = "Assets/_Project/Animation";

        public string DisplayName => "Animation";
        public string Icon => "▶";
        public int Order => 30;

        private readonly VisualElement root = new();

        private DatabaseBrowserView<AnimationProfile> browser;
        private VisualElement createPanel;
        private TextField nameField;
        private ObjectField characterField;
        private Label createError;

        public VisualElement BuildUI()
        {
            root.AddToClassList("anim-tool");
            StyleSheetLoader.Load(root, BrowserStylePath);
            StyleSheetLoader.Load(root, ToolStylePath);

            root.Add(BuildCreatePanel());

            browser = new DatabaseBrowserView<AnimationProfile>("Profile", ShowCreatePanel, BuildSelectionActions, ProfilesRoot);
            VisualElement browserContent = browser.Build();
            browserContent.AddToClassList("anim-tool__browser");
            root.Add(browserContent);

            root.Add(BuildSettings());
            return root;
        }

        public void OnActivated() => browser?.Reload();
        public void OnDeactivated() { }

        // ---- sélection ------------------------------------------------------

        private static void BuildSelectionActions(AnimationProfile profile, VisualElement container)
        {
            Button reconnect = new(() => Reconnect(profile)) { text = "Reconnect UMotion" };
            reconnect.AddToClassList("anim-tool__secondary-button");
            reconnect.tooltip = "Reload the UMotion project and reassign the character of the open stage.";
            container.Add(reconnect);

            Button open = new(() => AnimationStage.Open(profile)) { text = "Open Stage" };
            open.AddToClassList("anim-tool__primary-button");
            container.Add(open);
        }

        private static void Reconnect(AnimationProfile profile)
        {
            AnimationStage stage = AnimationStage.Current;
            if (stage == null || stage.Profile != profile)
            {
                Debug.LogWarning($"[AnimationTool] Open the stage of '{profile.name}' first.");
                return;
            }

            stage.ReconnectUMotion();
        }

        // ---- création -------------------------------------------------------

        private VisualElement BuildCreatePanel()
        {
            createPanel = new VisualElement();
            createPanel.AddToClassList("anim-tool__create");
            createPanel.style.display = DisplayStyle.None;

            Label title = new("New Animation Profile");
            title.AddToClassList("anim-tool__create-title");
            createPanel.Add(title);

            nameField = new TextField("Name");
            createPanel.Add(nameField);

            characterField = new ObjectField("Character")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
            };
            createPanel.Add(characterField);

            createError = new Label();
            createError.AddToClassList("anim-tool__create-error");
            createPanel.Add(createError);

            VisualElement buttons = new();
            buttons.AddToClassList("anim-tool__create-buttons");

            Button cancel = new(HideCreatePanel) { text = "Cancel" };
            cancel.AddToClassList("anim-tool__secondary-button");
            Button create = new(CreateProfile) { text = "Create" };
            create.AddToClassList("anim-tool__primary-button");

            buttons.Add(cancel);
            buttons.Add(create);
            createPanel.Add(buttons);

            return createPanel;
        }

        private void ShowCreatePanel()
        {
            nameField.SetValueWithoutNotify(string.Empty);
            characterField.SetValueWithoutNotify(null);
            createError.text = string.Empty;
            createPanel.style.display = DisplayStyle.Flex;
        }

        private void HideCreatePanel() => createPanel.style.display = DisplayStyle.None;

        private void CreateProfile()
        {
            string profileName = nameField.value?.Trim();
            GameObject character = characterField.value as GameObject;

            if (string.IsNullOrEmpty(profileName) || profileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                createError.text = "Enter a valid name.";
                return;
            }

            if (character == null)
            {
                createError.text = "Assign a character prefab.";
                return;
            }

            Object template = AnimationEditorSettings.GetOrCreate().UMotionTemplate;
            string templatePath = template != null ? AssetDatabase.GetAssetPath(template) : null;
            if (string.IsNullOrEmpty(templatePath))
            {
                createError.text = "Assign a UMotion template in the Settings section first.";
                return;
            }

            string folder = $"{ProfilesRoot}/{profileName}";
            if (AssetDatabase.IsValidFolder(folder))
            {
                createError.text = $"A profile folder named '{profileName}' already exists.";
                return;
            }

            AnimationEditorSettings.EnsureFolder(folder);

            string projectPath = $"{folder}/{profileName}Motion{Path.GetExtension(templatePath)}";
            if (!AssetDatabase.CopyAsset(templatePath, projectPath))
            {
                createError.text = "Failed to copy the UMotion template.";
                return;
            }

            AnimationProfile profile = ScriptableObject.CreateInstance<AnimationProfile>();
            profile.Initialize(character, AssetDatabase.LoadMainAssetAtPath(projectPath));
            AssetDatabase.CreateAsset(profile, $"{folder}/{profileName}.asset");
            AssetDatabase.SaveAssets();

            HideCreatePanel();
            browser.Reload();
            browser.SetSelection(profile);
        }

        // ---- settings -------------------------------------------------------

        private static VisualElement BuildSettings()
        {
            AnimationEditorSettings settings = AnimationEditorSettings.GetOrCreate();

            Foldout foldout = new() { text = "Settings", value = false };
            foldout.AddToClassList("anim-tool__settings");

            ObjectField templateField = new("UMotion Template")
            {
                objectType = typeof(Object),
                allowSceneObjects = false,
                value = settings.UMotionTemplate,
            };
            templateField.RegisterValueChangedCallback(e => settings.UMotionTemplate = e.newValue);
            foldout.Add(templateField);

            ObjectField environmentField = new("Environment Prefab")
            {
                objectType = typeof(GameObject),
                allowSceneObjects = false,
                value = settings.EnvironmentPrefab,
            };
            environmentField.RegisterValueChangedCallback(e => settings.EnvironmentPrefab = e.newValue as GameObject);
            foldout.Add(environmentField);

            Label note = new("Empty environment: a default light and ground are generated in the stage.");
            note.AddToClassList("anim-tool__note");
            foldout.Add(note);

            return foldout;
        }
    }
}
