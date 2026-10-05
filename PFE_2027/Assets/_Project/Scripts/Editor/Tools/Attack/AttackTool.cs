using System.Collections.Generic;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    // Attack tab: list of AttackData + "Edit" button. When an AttackStage is open, the tab switches
    // to edit mode: window tracks (hitboxes, attack windows) aligned with the playhead,
    // then the useful data fields (the Animation group is hidden).
    public sealed class AttackTool : IEditorTool
    {
        private const string BROWSER_STYLE_PATH = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";
        private const string STYLE_PATH = "Assets/_Project/Scripts/Editor/Tools/Attack/AttackTool.uss";
        private const string SEARCH_ROOT = "Assets/_Project";

        // Track color of an attack window, picked from its lowest flag bit
        private static readonly Color[] FlagColors =
        {
            new(0.6f, 0.6f, 0.65f),   // bit 0 - MovementLock
            new(0.35f, 0.6f, 1f),     // bit 1 - Combo
            new(0.75f, 0.55f, 0.35f), // bit 2
            new(0.4f, 0.8f, 0.45f),   // bit 3
            new(0.95f, 0.85f, 0.3f),  // bit 4
        };
        private static readonly Color NoFlagColor = new(0.35f, 0.35f, 0.35f);

        public string DisplayName => "Attack";
        public string Icon => "⚔";
        public int Order => 40;

        private readonly VisualElement root = new();
        private DatabaseBrowserView<AttackData> browser;
        private VisualElement browserContent;
        private VisualElement editPanel;

        // Editing
        private SerializedObject serializedAttack;
        private VisualElement trackContainer;
        private readonly List<TimeWindowTrack> tracks = new();
        private readonly List<VisualElement> hitboxRows = new();
        private int trackedHitboxCount;
        // Flags of every attack window when the tracks were built: a change rebuilds them (title, color)
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

            browser = new DatabaseBrowserView<AttackData>(DisplayName, buildSelectionActions: BuildSelectionActions,
                searchRoot: SEARCH_ROOT);
            browserContent = browser.Build();
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
            browser?.Reload();
            RefreshMode();
        }

        public void OnDeactivated() => StopPlayback();

        // ---------------------------------------------------------------- stage

        private void Subscribe()
        {
            Unsubscribe();
            AttackStage.OnOpened += OnStageOpened;
            AttackStage.OnClosed += OnStageClosed;
            AttackStage.OnSelectionChanged += OnSelectionChanged;
        }

        private void Unsubscribe()
        {
            AttackStage.OnOpened -= OnStageOpened;
            AttackStage.OnClosed -= OnStageClosed;
            AttackStage.OnSelectionChanged -= OnSelectionChanged;
        }

        private void OnStageOpened(AttackStage stage) => RefreshMode();

        // The stage SerializedObject is about to be released: clear the panel right away (otherwise change
        // tracking would read a released object), then check where we return to on the next frame.
        private void OnStageClosed()
        {
            ClearEditPanel();
            root.schedule.Execute(RefreshMode);
        }

        private void BuildSelectionActions(AttackData attack, VisualElement container)
        {
            Button editButton = new(() => AttackStage.Open(attack, AttackStage.FindPreviewCharacter(attack)))
                { text = "Edit" };
            editButton.AddToClassList("attack-tool__edit-button");
            container.Add(editButton);
        }

        private void RefreshMode()
        {
            AttackStage stage = AttackStage.Current;
            if (stage == null || stage.Attack == null || stage.Attack.AnimationClip == null)
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
            serializedAttack = null;
            frameSlider = null;
            timeLabel = null;
            playButton = null;
        }

        // ---------------------------------------------------------------- edit panel

        private void BuildEditPanel(AttackStage stage)
        {
            ClearEditPanel();
            serializedAttack = stage.SerializedAttack;
            serializedAttack.Update();

            // Fresh container on every rebuild: its change tracking goes away with it
            VisualElement content = new();
            content.AddToClassList("attack-edit");
            editPanel.Add(content);

            content.Add(BuildHeader(stage));
            content.Add(BuildTimeline(stage));
            content.Add(BuildFields());

            // Called on every change of the asset, wherever it comes from (fields, handles, tracks, Undo...)
            content.TrackSerializedObjectValue(serializedAttack, OnAttackChanged);
        }

        private static VisualElement BuildHeader(AttackStage stage)
        {
            VisualElement header = new();
            header.AddToClassList("attack-edit__header");

            Label title = new($"Editing: {stage.Attack.name}");
            title.AddToClassList("attack-edit__title");
            header.Add(title);

            VisualElement buttons = new();
            buttons.AddToClassList("attack-edit__header-buttons");
            buttons.Add(new Button(() =>
            {
                if (AttackStage.Current != null)
                    AttackStage.Current.SelectedHitbox = -1;
            }) { text = "Deselect", tooltip = "Stop editing the selected hitbox (Esc in the Scene view)." });
            buttons.Add(new Button(StageUtility.GoToMainStage) { text = "Back to Scene" });
            header.Add(buttons);

            return header;
        }

        private VisualElement BuildTimeline(AttackStage stage)
        {
            AnimationClip clip = stage.Attack.AnimationClip;
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
                AttackStage.Current?.Sample(evt.newValue / clip.frameRate);
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

            SerializedProperty hitboxes = serializedAttack.FindProperty(AttackPropertyPaths.Hitboxes);
            trackedHitboxCount = hitboxes.arraySize;

            for (int i = 0; i < hitboxes.arraySize; i++)
            {
                SerializedProperty window = hitboxes.GetArrayElementAtIndex(i).FindPropertyRelative(AttackPropertyPaths.Window);
                AddTrackRow($"Hitbox #{i}", window, HitboxDrawer.GetColor(i), i);
            }

            SerializedProperty windows = serializedAttack.FindProperty(AttackPropertyPaths.Windows);
            trackedWindowFlags.Clear();

            for (int i = 0; i < windows.arraySize; i++)
            {
                SerializedProperty element = windows.GetArrayElementAtIndex(i);
                int flags = element.FindPropertyRelative(AttackPropertyPaths.Flags).intValue;
                trackedWindowFlags.Add(flags);

                AddTrackRow(GetWindowTitle((AttackFlags)flags),
                    element.FindPropertyRelative(AttackPropertyPaths.Window), GetWindowColor((AttackFlags)flags), -1);
            }

            OnSelectionChanged(AttackStage.Current != null ? AttackStage.Current.SelectedHitbox : -1);

            AttackStage stage = AttackStage.Current;
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
                    if (AttackStage.Current != null)
                        AttackStage.Current.SelectedHitbox = hitboxIndex;
                });
                hitboxRows.Add(row);
            }

            TimeWindowTrack track = new(window, color, totalFrames);
            row.Add(track);
            tracks.Add(track);
            trackContainer.Add(row);
        }

        // Only the fields useful while editing: controller, state and clip stay in the regular inspector
        private VisualElement BuildFields()
        {
            ScrollView scroll = new(ScrollViewMode.Vertical);
            scroll.AddToClassList("attack-edit__inspector");

            scroll.Add(new PropertyField(serializedAttack.FindProperty(AttackPropertyPaths.Hitboxes), "Hitboxes"));
            scroll.Add(new PropertyField(serializedAttack.FindProperty(AttackPropertyPaths.Windows), "Windows"));
            scroll.Add(new PropertyField(serializedAttack.FindProperty(AttackPropertyPaths.Next), "Next"));

            scroll.Bind(serializedAttack);
            return scroll;
        }

        private void OnAttackChanged(SerializedObject _)
        {
            if (serializedAttack == null || trackContainer == null)
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
            if (serializedAttack.FindProperty(AttackPropertyPaths.Hitboxes).arraySize != trackedHitboxCount)
                return true;

            SerializedProperty windows = serializedAttack.FindProperty(AttackPropertyPaths.Windows);
            if (windows.arraySize != trackedWindowFlags.Count)
                return true;

            for (int i = 0; i < windows.arraySize; i++)
            {
                if (windows.GetArrayElementAtIndex(i).FindPropertyRelative(AttackPropertyPaths.Flags).intValue != trackedWindowFlags[i])
                    return true;
            }

            return false;
        }

        private static string GetWindowTitle(AttackFlags flags) =>
            flags == AttackFlags.None ? "(no flag)" : flags.ToString();

        private static Color GetWindowColor(AttackFlags flags)
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

        private static int GetFrame(AttackStage stage) =>
            Mathf.RoundToInt(stage.CurrentSeconds * stage.Attack.AnimationClip.frameRate);

        private void StepFrame(int direction)
        {
            AttackStage stage = AttackStage.Current;
            if (stage == null || frameSlider == null)
                return;

            StopPlayback();
            AnimationClip clip = stage.Attack.AnimationClip;
            int frame = Mathf.Clamp(GetFrame(stage) + direction, 0, GetTotalFrames(clip));
            stage.Sample(frame / clip.frameRate);
            frameSlider.SetValueWithoutNotify(frame);
            UpdateTimeLabel();
        }

        private void UpdateTimeLabel()
        {
            AttackStage stage = AttackStage.Current;
            if (stage == null || timeLabel == null)
                return;

            AnimationClip clip = stage.Attack.AnimationClip;
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
            AttackStage stage = AttackStage.Current;
            if (stage == null || stage.Attack == null || stage.Attack.AnimationClip == null || frameSlider == null)
            {
                StopPlayback();
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            float delta = (float)(now - lastTickTime);
            lastTickTime = now;

            float length = stage.Attack.AnimationClip.length;
            stage.Sample(Mathf.Repeat(stage.CurrentSeconds + delta, length)); // loop

            frameSlider.SetValueWithoutNotify(GetFrame(stage));
            UpdateTimeLabel();
        }
    }
}
