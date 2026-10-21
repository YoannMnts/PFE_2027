using System;
using System.Collections.Generic;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Fight.Dashes;
using PFE.Editor._Project.Scripts.Editor.Tools.Animation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Hitbox
{
    // Hitbox tab: one sub-tab per HitboxData type (Attack, Pushback...), each listing its assets with an
    // "Edit" button. When a HitboxStage is open, the tab switches to edit mode: window tracks aligned with
    // the playhead, then the hitboxes, windows and the fields specific to the edited type.
    public sealed class HitboxTool : IEditorTool
    {
        private const string BROWSER_STYLE_PATH = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";
        private const string STYLE_PATH = "Assets/_Project/Scripts/Editor/Tools/Hitbox/HitboxTool.uss";
        private const string SEARCH_ROOT = "Assets/_Project";
        private const string ATTACK_FOLDER = "Assets/_Project/Resources/Database/Hitboxes/Basics";
        private const string PUSHBACK_FOLDER = "Assets/_Project/Resources/Database/Hitboxes/Pushbacks";
        private const string DASH_FOLDER = "Assets/_Project/Resources/Database/Hitboxes/Dashes";
        private const string SELECTED_CATEGORY_SESSION_KEY = "PFE.Editor.Hitbox.SelectedCategory";
        private const string PREVIEW_PROFILE_PREFS_KEY = "PFE.Editor.Hitbox.PreviewProfile.";

        // Track color of an additional window, picked from the type of its flag
        private static readonly Dictionary<string, Color> FlagColors = new()
        {
            { nameof(MovementLock), new Color(0.6f, 0.6f, 0.65f) },
            { nameof(Combo), new Color(0.35f, 0.6f, 1f) },
            { nameof(Displacement), new Color(0.85f, 0.45f, 0.75f) },
        };

        // Flags without a color of their own (VFX, SFX...) get one of these, always the same for a given type
        private static readonly Color[] FallbackFlagColors =
        {
            new(0.75f, 0.55f, 0.35f),
            new(0.4f, 0.8f, 0.45f),
            new(0.95f, 0.85f, 0.3f),
        };
        private static readonly Color NoFlagColor = new(0.35f, 0.35f, 0.35f);

        public string DisplayName => "Hitbox";
        public string Icon => "⚔";
        public int Order => 40;

        // One sub-tab per HitboxData type: add a line here for a new type
        private readonly HitboxCategory[] categories =
        {
            new HitboxCategory<AttackData>("Attack", ATTACK_FOLDER),
            new HitboxCategory<PushbackData>("Pushback", PUSHBACK_FOLDER),
            new HitboxCategory<DashData>("Dash", DASH_FOLDER),
        };

        private readonly VisualElement root = new();
        private VisualElement browserContent;
        private VisualElement editPanel;

        // Sub-tabs
        private readonly List<Button> categoryButtons = new();
        private readonly List<VisualElement> categoryContents = new();
        private int selectedCategory;

        // Editing
        private SerializedObject serializedData;
        private VisualElement trackContainer;
        private readonly List<TimeWindowTrack> tracks = new();
        private readonly List<VisualElement> hitboxRows = new();
        private int trackedHitboxCount;
        // Flag type of every additional window when the tracks were built: a change rebuilds them (title, color)
        private readonly List<string> trackedWindowFlags = new();
        private int totalFrames;

        // Knockback redirect section (PushbackData only)
        private VisualElement redirectSection;
        private KnockbackRedirectData shownRedirect;

        // Playhead
        private SliderInt frameSlider;
        private Label timeLabel;
        private Button playButton;
        private bool isPlaying;
        private double lastTickTime;

        public VisualElement BuildUI()
        {
            root.AddToClassList("attack-tool");
            StyleSheetLoader.Load(root, BROWSER_STYLE_PATH);
            StyleSheetLoader.Load(root, STYLE_PATH);

            browserContent = BuildBrowsers();
            root.Add(browserContent);

            editPanel = new VisualElement();
            editPanel.AddToClassList("attack-edit");
            root.Add(editPanel);

            root.RegisterCallback<AttachToPanelEvent>(_ => Subscribe());
            root.RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                Unsubscribe();
                StopPlayback();
            });

            RefreshMode();
            return root;
        }

        public void OnActivated()
        {
            categories[selectedCategory].Reload();
            RefreshMode();
        }

        public void OnDeactivated() => StopPlayback();

        // ---------------------------------------------------------------- sub-tabs

        private VisualElement BuildBrowsers()
        {
            VisualElement container = new();
            container.AddToClassList("hitbox-browsers");

            VisualElement bar = new();
            bar.AddToClassList("hitbox-tabs");
            container.Add(bar);

            for (int i = 0; i < categories.Length; i++)
            {
                int index = i;
                Button button = new(() => SelectCategory(index)) { text = categories[i].Title };
                button.AddToClassList("hitbox-tabs__tab");
                bar.Add(button);
                categoryButtons.Add(button);

                VisualElement content = categories[i].Build(BuildSelectionActions, SEARCH_ROOT);
                content.AddToClassList("hitbox-category");
                container.Add(content);
                categoryContents.Add(content);
            }

            int saved = SessionState.GetInt(SELECTED_CATEGORY_SESSION_KEY, 0);
            SelectCategory(Mathf.Clamp(saved, 0, categories.Length - 1));
            return container;
        }

        private void SelectCategory(int index)
        {
            selectedCategory = index;
            SessionState.SetInt(SELECTED_CATEGORY_SESSION_KEY, index);

            for (int i = 0; i < categories.Length; i++)
            {
                bool isSelected = i == index;
                categoryButtons[i].EnableInClassList("hitbox-tabs__tab--active", isSelected);
                categoryContents[i].style.display = isSelected ? DisplayStyle.Flex : DisplayStyle.None;
            }

            categories[index].Reload();
        }

        // ---------------------------------------------------------------- stage

        private void Subscribe()
        {
            Unsubscribe();
            HitboxStage.OnOpened += OnStageOpened;
            HitboxStage.OnClosed += OnStageClosed;
            HitboxStage.OnSelectionChanged += OnSelectionChanged;
        }

        private void Unsubscribe()
        {
            HitboxStage.OnOpened -= OnStageOpened;
            HitboxStage.OnClosed -= OnStageClosed;
            HitboxStage.OnSelectionChanged -= OnSelectionChanged;
        }

        private void OnStageOpened(HitboxStage stage) => RefreshMode();

        // The stage SerializedObject is about to be released: clear the panel right away (otherwise change
        // tracking would read a released object), then check where we return to on the next frame.
        private void OnStageClosed()
        {
            ClearEditPanel();
            root.schedule.Execute(RefreshMode);
        }

        // Preview character chosen per asset (editor preference: the runtime data knows nothing about it)
        private void BuildSelectionActions(HitboxData data, VisualElement container)
        {
            List<AnimationProfile> profiles = FindAnimationProfiles();
            if (profiles.Count == 0)
            {
                container.Add(new Label("No AnimationProfile: create one to preview this animation."));
                return;
            }

            string prefsKey = PREVIEW_PROFILE_PREFS_KEY + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));
            string savedGuid = EditorPrefs.GetString(prefsKey, string.Empty);
            AnimationProfile selected = profiles.Find(p => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(p)) == savedGuid)
                                        ?? profiles[0];

            PopupField<AnimationProfile> previewField = new("Preview", profiles, selected, p => p.name, p => p.name);
            previewField.RegisterValueChangedCallback(evt =>
                EditorPrefs.SetString(prefsKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(evt.newValue))));
            container.Add(previewField);

            Button editButton = new(() => HitboxStage.Open(data, previewField.value.Character)) { text = "Edit" };
            editButton.AddToClassList("attack-tool__edit-button");
            container.Add(editButton);
        }

        private static List<AnimationProfile> FindAnimationProfiles()
        {
            List<AnimationProfile> profiles = new();
            foreach (string guid in AssetDatabase.FindAssets("t:AnimationProfile"))
            {
                var profile = AssetDatabase.LoadAssetAtPath<AnimationProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if (profile != null && profile.Character != null)
                    profiles.Add(profile);
            }
            return profiles;
        }

        private void RefreshMode()
        {
            HitboxStage stage = HitboxStage.Current;
            if (stage == null || stage.Data == null || stage.Data.AnimationClip == null)
            {
                ClearEditPanel();
                browserContent.style.display = DisplayStyle.Flex;
                editPanel.style.display = DisplayStyle.None;
                return;
            }

            BuildEditPanel(stage);
            browserContent.style.display = DisplayStyle.None;
            editPanel.style.display = DisplayStyle.Flex;
        }

        private void ClearEditPanel()
        {
            StopPlayback();
            editPanel.Clear();
            tracks.Clear();
            hitboxRows.Clear();
            trackedWindowFlags.Clear();
            serializedData = null;
            frameSlider = null;
            timeLabel = null;
            playButton = null;
            redirectSection = null;
            shownRedirect = null;
            windowsSection = null;
        }

        // ---------------------------------------------------------------- edit panel

        private void BuildEditPanel(HitboxStage stage)
        {
            ClearEditPanel();
            serializedData = stage.SerializedData;
            serializedData.Update();

            // Fresh container on every rebuild: its change tracking goes away with it.
            // The whole panel scrolls (header, timeline and fields).
            ScrollView content = new(ScrollViewMode.Vertical);
            content.AddToClassList("attack-edit");
            editPanel.Add(content);

            content.Add(BuildHeader(stage));
            content.Add(BuildTimeline(stage));
            content.Add(BuildFields());

            // Called on every change of the asset, wherever it comes from (fields, handles, tracks, Undo...)
            content.TrackSerializedObjectValue(serializedData, OnDataChanged);
        }

        private static VisualElement BuildHeader(HitboxStage stage)
        {
            VisualElement header = new();
            header.AddToClassList("attack-edit__header");

            Label title = new($"Editing: {stage.Data.name}");
            title.AddToClassList("attack-edit__title");
            header.Add(title);

            VisualElement buttons = new();
            buttons.AddToClassList("attack-edit__header-buttons");
            buttons.Add(new Button(() =>
            {
                if (HitboxStage.Current != null)
                    HitboxStage.Current.SelectedHitbox = -1;
            }) { text = "Deselect", tooltip = "Stop editing the selected hitbox (Esc in the Scene view)." });
            buttons.Add(new Button(StageUtility.GoToMainStage) { text = "Back to Scene" });
            header.Add(buttons);

            return header;
        }

        private VisualElement BuildTimeline(HitboxStage stage)
        {
            AnimationClip clip = stage.Data.AnimationClip;
            totalFrames = GetTotalFrames(clip);

            VisualElement timeline = new();
            timeline.AddToClassList("attack-timeline");

            trackContainer = new VisualElement();
            timeline.Add(trackContainer);
            BuildTracks();

            // Slider row: same structure as a track (fixed title + stretching area) for alignment
            VisualElement sliderRow = new();
            sliderRow.AddToClassList("attack-track-row");

            Label frameTitle = new("Frame");
            frameTitle.AddToClassList("attack-track-row__label");
            sliderRow.Add(frameTitle);

            frameSlider = new SliderInt(0, totalFrames);
            frameSlider.AddToClassList("attack-timeline__slider");
            frameSlider.SetValueWithoutNotify(GetFrame(stage));
            frameSlider.RegisterValueChangedCallback(evt =>
            {
                StopPlayback();
                HitboxStage.Current?.Sample(evt.newValue / clip.frameRate);
                UpdateTimeLabel();
            });
            sliderRow.Add(frameSlider);
            timeline.Add(sliderRow);

            VisualElement controls = new();
            controls.AddToClassList("attack-timeline__controls");
            controls.Add(new Button(() => StepFrame(-1)) { text = "◀" });
            playButton = new Button(TogglePlayback) { text = "Play" };
            controls.Add(playButton);
            controls.Add(new Button(() => StepFrame(+1)) { text = "▶" });

            timeLabel = new Label();
            timeLabel.AddToClassList("attack-timeline__label");
            controls.Add(timeLabel);
            timeline.Add(controls);

            UpdateTimeLabel();
            return timeline;
        }

        private void BuildTracks()
        {
            trackContainer.Clear();
            tracks.Clear();
            hitboxRows.Clear();

            SerializedProperty hitboxes = serializedData.FindProperty(HitboxPropertyPaths.Hitboxes);
            trackedHitboxCount = hitboxes.arraySize;

            for (int i = 0; i < hitboxes.arraySize; i++)
            {
                SerializedProperty window = hitboxes.GetArrayElementAtIndex(i).FindPropertyRelative(HitboxPropertyPaths.Window);
                AddTrackRow($"Hitbox #{i}", window, HitboxDrawer.GetColor(i), i);
            }

            SerializedProperty windows = serializedData.FindProperty(HitboxPropertyPaths.Windows);
            trackedWindowFlags.Clear();

            for (int i = 0; i < windows.arraySize; i++)
            {
                SerializedProperty element = windows.GetArrayElementAtIndex(i);
                string flagType = GetFlagTypeName(element);
                trackedWindowFlags.Add(flagType);

                AddTrackRow(GetWindowTitle(flagType),
                    element.FindPropertyRelative(HitboxPropertyPaths.Window), GetWindowColor(flagType), -1);
            }

            OnSelectionChanged(HitboxStage.Current != null ? HitboxStage.Current.SelectedHitbox : -1);

            HitboxStage stage = HitboxStage.Current;
            if (stage != null)
                foreach (TimeWindowTrack track in tracks)
                    track.SetPlayhead(stage.NormalizedTime);
        }

        private void AddTrackRow(string title, SerializedProperty window, Color color, int hitboxIndex)
        {
            VisualElement row = new();
            row.AddToClassList("attack-track-row");

            Label label = new(title) { tooltip = title };
            label.AddToClassList("attack-track-row__label");
            row.Add(label);

            if (hitboxIndex >= 0)
            {
                // Clicking a hitbox title selects it for the scene handles
                label.AddToClassList("attack-track-row__label--selectable");
                label.RegisterCallback<ClickEvent>(_ =>
                {
                    if (HitboxStage.Current != null)
                        HitboxStage.Current.SelectedHitbox = hitboxIndex;
                });
                hitboxRows.Add(row);
            }

            TimeWindowTrack track = new(window, color, totalFrames);
            row.Add(track);
            tracks.Add(track);
            trackContainer.Add(row);
        }

        // Hitboxes and windows first, then every field declared by the edited type (AttackData, PushbackData...).
        // The Animation group (controller, state, clip) stays in the regular inspector.
        private VisualElement BuildFields()
        {
            VisualElement fields = new();
            fields.AddToClassList("attack-edit__inspector");

            fields.Add(new PropertyField(serializedData.FindProperty(HitboxPropertyPaths.Hitboxes), "Hitboxes"));

            windowsSection = new VisualElement();
            windowsSection.AddToClassList("attack-edit__section");
            fields.Add(windowsSection);
            RebuildWindowsSection();

            SerializedProperty iterator = serializedData.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (HitboxPropertyPaths.IsBaseField(iterator.propertyPath))
                    continue;

                fields.Add(new PropertyField(iterator.Copy()));
            }

            // The cone is a separate shared asset: its inspector is shown under the pushback fields
            if (serializedData.targetObject is PushbackData)
            {
                redirectSection = new VisualElement();
                redirectSection.AddToClassList("attack-edit__section");
                fields.Add(redirectSection);
                RefreshRedirectSection();
            }

            fields.Bind(serializedData);
            return fields;
        }

        private void OnDataChanged(SerializedObject _)
        {
            if (serializedData == null || trackContainer == null)
                return;

            // Structure changed (hitbox/window added or removed, flags edited): rebuild; otherwise just refresh the bars
            if (HasTrackLayoutChanged())
            {
                BuildTracks();
                RebuildWindowsSection();
            }
            else
                foreach (TimeWindowTrack track in tracks)
                    track.Refresh();

            RefreshRedirectSection();

            // Distances, curves or windows may have changed: put the character back where the playhead says
            HitboxStage stage = HitboxStage.Current;
            if (stage != null)
                stage.Sample(stage.CurrentSeconds);

            SceneView.RepaintAll();
        }

        // Rebuilt only when another cone asset is assigned (or removed)
        private void RefreshRedirectSection()
        {
            if (redirectSection == null || serializedData.targetObject is not PushbackData pushback)
                return;

            KnockbackRedirectData redirect = pushback.Redirect;
            if (redirect == shownRedirect && redirectSection.childCount > 0)
                return;

            shownRedirect = redirect;
            redirectSection.Clear();

            Label title = new("Knockback Redirect");
            title.AddToClassList("attack-edit__section-title");
            redirectSection.Add(title);

            if (redirect == null)
            {
                redirectSection.Add(new HelpBox(
                    "No cone assigned: the knockback goes straight. Assign a Knockback Redirect asset in the Redirect field above to edit it here and in the Scene view.",
                    HelpBoxMessageType.Info));
                return;
            }

            redirectSection.Add(new HelpBox(
                $"Shared asset '{redirect.name}': changes apply to every pushback using it.", HelpBoxMessageType.None));
            redirectSection.Add(new InspectorElement(redirect));
        }

        private bool HasTrackLayoutChanged()
        {
            if (serializedData.FindProperty(HitboxPropertyPaths.Hitboxes).arraySize != trackedHitboxCount)
                return true;

            SerializedProperty windows = serializedData.FindProperty(HitboxPropertyPaths.Windows);
            if (windows.arraySize != trackedWindowFlags.Count)
                return true;

            for (int i = 0; i < windows.arraySize; i++)
            {
                if (GetFlagTypeName(windows.GetArrayElementAtIndex(i)) != trackedWindowFlags[i])
                    return true;
            }

            return false;
        }

        // Windows section: built by hand because the default PropertyField has no type picker for the
        // [SerializeReference] flag (Odin draws one in the regular inspector, not in UI Toolkit)
        private VisualElement windowsSection;

        // Every concrete IWindowFlag type (found once per domain reload), offered by the flag dropdown
        private static List<Type> flagTypes;
        private static List<string> flagChoices;

        private static List<Type> FlagTypes => flagTypes ??= FindFlagTypes();

        private static List<string> FlagChoices
        {
            get
            {
                if (flagChoices != null)
                    return flagChoices;

                flagChoices = new List<string> { "(no flag)" };
                foreach (Type type in FlagTypes)
                    flagChoices.Add(ObjectNames.NicifyVariableName(type.Name));

                return flagChoices;
            }
        }

        private static List<Type> FindFlagTypes()
        {
            List<Type> types = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IWindowFlag>())
            {
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition
                    || typeof(UnityEngine.Object).IsAssignableFrom(type))
                    continue;

                // Activator needs a parameterless constructor (always there for a struct)
                if (!type.IsValueType && type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                types.Add(type);
            }

            types.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return types;
        }

        // Same format as SerializedProperty.managedReferenceFullTypename: "Assembly Namespace.Type"
        private static string GetManagedReferenceName(Type type) =>
            $"{type.Assembly.GetName().Name} {type.FullName?.Replace('+', '/')}";

        private void RebuildWindowsSection()
        {
            if (windowsSection == null || serializedData == null)
                return;

            windowsSection.Clear();

            Label title = new("Windows");
            title.AddToClassList("attack-edit__section-title");
            windowsSection.Add(title);

            SerializedProperty windows = serializedData.FindProperty(HitboxPropertyPaths.Windows);
            for (int i = 0; i < windows.arraySize; i++)
                windowsSection.Add(BuildWindowElement(windows.GetArrayElementAtIndex(i), i));

            windowsSection.Add(new Button(AddWindow) { text = "Add window" });
            windowsSection.Bind(serializedData);
        }

        private VisualElement BuildWindowElement(SerializedProperty element, int index)
        {
            VisualElement box = new();
            box.AddToClassList("attack-edit__window");

            SerializedProperty flag = element.FindPropertyRelative(HitboxPropertyPaths.Flag);
            string flagPath = flag.propertyPath;
            string currentType = flag.managedReferenceFullTypename;

            // Header: type of the flag (a change creates a fresh instance) and remove button
            VisualElement header = new();
            header.AddToClassList("attack-edit__window-header");

            int currentIndex = FlagTypes.FindIndex(type => GetManagedReferenceName(type) == currentType) + 1;
            DropdownField typeField = new($"Window #{index}", FlagChoices, currentIndex);
            typeField.AddToClassList("attack-edit__window-type");
            typeField.RegisterValueChangedCallback(_ => SetFlagType(flagPath, typeField.index));
            header.Add(typeField);
            header.Add(new Button(() => RemoveWindow(index)) { text = "✕", tooltip = "Remove this window." });
            box.Add(header);

            box.Add(new PropertyField(element.FindPropertyRelative(HitboxPropertyPaths.Window)));

            // Values of the flag (Combo.NextAttack, Displacement.Distance...)
            if (!string.IsNullOrEmpty(currentType))
            {
                SerializedProperty child = flag.Copy();
                SerializedProperty end = flag.GetEndProperty();
                bool enterChildren = true;
                while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
                {
                    enterChildren = false;
                    box.Add(new PropertyField(child.Copy()));
                }
            }

            return box;
        }

        // A new flag type = a fresh instance with default values; "(no flag)" clears it
        private void SetFlagType(string flagPath, int choiceIndex)
        {
            SerializedProperty flag = serializedData?.FindProperty(flagPath);
            if (flag == null)
                return;

            flag.managedReferenceValue = choiceIndex > 0 ? Activator.CreateInstance(FlagTypes[choiceIndex - 1]) : null;
            serializedData.ApplyModifiedProperties();
        }

        private void AddWindow()
        {
            SerializedProperty windows = serializedData.FindProperty(HitboxPropertyPaths.Windows);
            int index = windows.arraySize;
            windows.InsertArrayElementAtIndex(index);

            // The new element copies the previous one: give it no flag instead of sharing the same reference
            windows.GetArrayElementAtIndex(index).FindPropertyRelative(HitboxPropertyPaths.Flag).managedReferenceValue = null;
            serializedData.ApplyModifiedProperties();
        }

        private void RemoveWindow(int index)
        {
            SerializedProperty windows = serializedData.FindProperty(HitboxPropertyPaths.Windows);
            if (index < 0 || index >= windows.arraySize)
                return;

            windows.DeleteArrayElementAtIndex(index);
            serializedData.ApplyModifiedProperties();
        }

        // Short class name of the flag held by a window (empty when none), read from its managed reference type
        // ("Assembly Namespace.Type"): no instance is created
        private static string GetFlagTypeName(SerializedProperty window)
        {
            string fullName = window.FindPropertyRelative(HitboxPropertyPaths.Flag).managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullName))
                return string.Empty;

            int separator = Mathf.Max(fullName.LastIndexOf('.'), fullName.LastIndexOf(' '));
            return fullName.Substring(separator + 1);
        }

        private static string GetWindowTitle(string flagType) =>
            string.IsNullOrEmpty(flagType) ? "(no flag)" : ObjectNames.NicifyVariableName(flagType);

        private static Color GetWindowColor(string flagType)
        {
            if (string.IsNullOrEmpty(flagType))
                return NoFlagColor;

            if (FlagColors.TryGetValue(flagType, out Color color))
                return color;

            // Stable across sessions (string.GetHashCode isn't guaranteed to be)
            int hash = 0;
            foreach (char character in flagType)
                hash = hash * 31 + character;

            return FallbackFlagColors[(hash & int.MaxValue) % FallbackFlagColors.Length];
        }

        private void OnSelectionChanged(int selected)
        {
            for (int i = 0; i < hitboxRows.Count; i++)
                hitboxRows[i].EnableInClassList("attack-track-row--selected", i == selected);
        }

        // ---------------------------------------------------------------- time helpers

        private static int GetTotalFrames(AnimationClip clip) =>
            Mathf.Max(1, Mathf.RoundToInt(clip.length * clip.frameRate));

        private static int GetFrame(HitboxStage stage) =>
            Mathf.RoundToInt(stage.CurrentSeconds * stage.Data.AnimationClip.frameRate);

        private void StepFrame(int direction)
        {
            HitboxStage stage = HitboxStage.Current;
            if (stage == null || frameSlider == null)
                return;

            StopPlayback();
            AnimationClip clip = stage.Data.AnimationClip;
            int frame = Mathf.Clamp(GetFrame(stage) + direction, 0, GetTotalFrames(clip));
            stage.Sample(frame / clip.frameRate);
            frameSlider.SetValueWithoutNotify(frame);
            UpdateTimeLabel();
        }

        private void UpdateTimeLabel()
        {
            HitboxStage stage = HitboxStage.Current;
            if (stage == null || timeLabel == null)
                return;

            AnimationClip clip = stage.Data.AnimationClip;
            timeLabel.text =
                $"{GetFrame(stage)} / {GetTotalFrames(clip)}   ·   t = {stage.NormalizedTime:0.00}   ·   {stage.CurrentSeconds:0.00}s";

            foreach (TimeWindowTrack track in tracks)
                track.SetPlayhead(stage.NormalizedTime);
        }

        // ---------------------------------------------------------------- playback

        private void TogglePlayback()
        {
            if (isPlaying)
                StopPlayback();
            else
                StartPlayback();
        }

        private void StartPlayback()
        {
            if (isPlaying)
                return;

            isPlaying = true;
            lastTickTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += PlaybackTick;
            if (playButton != null)
                playButton.text = "Pause";
        }

        private void StopPlayback()
        {
            if (!isPlaying)
                return;

            isPlaying = false;
            EditorApplication.update -= PlaybackTick;
            if (playButton != null)
                playButton.text = "Play";
        }

        private void PlaybackTick()
        {
            HitboxStage stage = HitboxStage.Current;
            if (stage == null || stage.Data == null || stage.Data.AnimationClip == null || frameSlider == null)
            {
                StopPlayback();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float delta = (float)(now - lastTickTime);
            lastTickTime = now;

            float length = stage.Data.AnimationClip.length;
            stage.Sample(Mathf.Repeat(stage.CurrentSeconds + delta, length)); // loop

            frameSlider.SetValueWithoutNotify(GetFrame(stage));
            UpdateTimeLabel();
        }
    }
}
