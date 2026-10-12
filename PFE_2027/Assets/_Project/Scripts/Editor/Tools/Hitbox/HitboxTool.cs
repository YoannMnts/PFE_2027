using System.Collections.Generic;
using PFE.Core.Scripts.Enemy.Attacks;
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
        private const string SELECTED_CATEGORY_SESSION_KEY = "PFE.Editor.Hitbox.SelectedCategory";

        // Track color of an additional window, picked from its lowest flag bit
        private static readonly Color[] FlagColors =
        {
            new(0.6f, 0.6f, 0.65f),   // bit 0 - MovementLock
            new(0.35f, 0.6f, 1f),     // bit 1 - Combo
            new(0.75f, 0.55f, 0.35f), // bit 2
            new(0.4f, 0.8f, 0.45f),   // bit 3
            new(0.95f, 0.85f, 0.3f),  // bit 4
        };
        private static readonly Color NoFlagColor = new(0.35f, 0.35f, 0.35f);

        public string DisplayName => "Hitbox";
        public string Icon => "⚔";
        public int Order => 40;

        // One sub-tab per HitboxData type: add a line here for a new type
        private readonly HitboxCategory[] categories =
        {
            new HitboxCategory<AttackData>("Attack"),
            new HitboxCategory<PushbackData>("Pushback"),
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
        // Flags of every additional window when the tracks were built: a change rebuilds them (title, color)
        private readonly List<int> trackedWindowFlags = new();
        private int totalFrames;

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

        private void BuildSelectionActions(HitboxData data, VisualElement container)
        {
            Button editButton = new(() => HitboxStage.Open(data, HitboxStage.FindPreviewCharacter(data)))
                { text = "Edit" };
            editButton.AddToClassList("attack-tool__edit-button");
            container.Add(editButton);
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
        }

        // ---------------------------------------------------------------- edit panel

        private void BuildEditPanel(HitboxStage stage)
        {
            ClearEditPanel();
            serializedData = stage.SerializedData;
            serializedData.Update();

            // Fresh container on every rebuild: its change tracking goes away with it
            VisualElement content = new();
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
                int flags = element.FindPropertyRelative(HitboxPropertyPaths.Flags).intValue;
                trackedWindowFlags.Add(flags);

                AddTrackRow(GetWindowTitle((WindowFlags)flags),
                    element.FindPropertyRelative(HitboxPropertyPaths.Window), GetWindowColor((WindowFlags)flags), -1);
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
            ScrollView scroll = new(ScrollViewMode.Vertical);
            scroll.AddToClassList("attack-edit__inspector");

            scroll.Add(new PropertyField(serializedData.FindProperty(HitboxPropertyPaths.Hitboxes), "Hitboxes"));
            scroll.Add(new PropertyField(serializedData.FindProperty(HitboxPropertyPaths.Windows), "Windows"));

            SerializedProperty iterator = serializedData.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (HitboxPropertyPaths.IsBaseField(iterator.propertyPath))
                    continue;

                scroll.Add(new PropertyField(iterator.Copy()));
            }

            scroll.Bind(serializedData);
            return scroll;
        }

        private void OnDataChanged(SerializedObject _)
        {
            if (serializedData == null || trackContainer == null)
                return;

            // Structure changed (hitbox/window added or removed, flags edited): rebuild; otherwise just refresh the bars
            if (HasTrackLayoutChanged())
                BuildTracks();
            else
                foreach (TimeWindowTrack track in tracks)
                    track.Refresh();

            SceneView.RepaintAll();
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
                if (windows.GetArrayElementAtIndex(i).FindPropertyRelative(HitboxPropertyPaths.Flags).intValue != trackedWindowFlags[i])
                    return true;
            }

            return false;
        }

        private static string GetWindowTitle(WindowFlags flags) =>
            flags == WindowFlags.None ? "(no flag)" : flags.ToString();

        private static Color GetWindowColor(WindowFlags flags)
        {
            int value = (int)flags;
            if (value == 0)
                return NoFlagColor;

            int bit = 0;
            while ((value & (1 << bit)) == 0)
                bit++;

            return FlagColors[bit % FlagColors.Length];
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
