// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the Synty Studios End User Licence Agreement (EULA)
// available at: https://syntystore.com/pages/end-user-licence-agreement
//
// For additional details, see the LICENSE.MD file bundled with this software.

#if UNITY_EDITOR

using Synty.SidekickCharacters.API;
using Synty.SidekickCharacters.Database;
using Synty.SidekickCharacters.Database.DTO;
using Synty.SidekickCharacters.Enums;
using Synty.SidekickCharacters.Filters;
using Synty.SidekickCharacters.Serialization;
using Synty.SidekickCharacters.SkinnedMesh;
using Synty.SidekickCharacters.Synty.SidekickCharacters.Scripts.Editor.UI;
using Synty.SidekickCharacters.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Serialization;
using UnityEditor;
using UnityEditor.Animations;
#if FBX_EXPORTER_AVAILABLE
using UnityEditor.Formats.Fbx.Exporter;
#endif
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Synty.SidekickCharacters
{
    public class ModularCharacterWindow : EditorWindow
    {
        private const string _AUTOSAVE_KEY = "SK_Autosave_character";
        private const string _AUTOSAVE_MISSING_PARTS = "SK_Autosave_missing_parts";
        private const string _AUTOSAVE_STATE = "SK_Autosave_state";
        private const string _BASE_COLOR_SET_NAME = "Species";
        private const string _BASE_COLOR_SET_PATH = "Assets/Synty/SidekickCharacters/Resources/Species";
        private const string _BASE_MESH_NAME = "Meshes/SK_BaseModel";
        private const string _BASE_MATERIAL_NAME = "Materials/M_BaseMaterial";
        private const string _BLEND_GENDER_NAME = "masculineFeminine";
        private const string _BLEND_MUSCLE_NAME = "defaultBuff";
        private const string _BLEND_SHAPE_HEAVY_NAME = "defaultHeavy";
        private const string _BLEND_SHAPE_SKINNY_NAME = "defaultSkinny";
        private const string _AUTO_OPEN_STATE = "syntySkAutoOpenState";
        private const string _COMBINE_MESHES_STATE = "SK_Combine_meshes";
        private const string _COMBINE_BODY_BLENDS_STATE = "SK_Combine_body_blends";
        private const string _COMBINE_FACIAL_BLENDS_STATE = "SK_Combine_facial_blends";
        private const string _FORCE_BASE_MODEL_STATE = "SK_Force_base_model";
        private const string _VERSION_FILE_PATH = "Assets/Synty/SidekickCharacters/Scripts/Editor/version.txt";
        private const string _OUTPUT_MODEL_NAME = "Combined Character";
        private const string _PART_COUNT_BODY = " parts in library";
        private const string _TEXTURE_COLOR_NAME = "ColorMap.png";
        private const string _TEXTURE_METALLIC_NAME = "MetallicMap.png";
        private const string _TEXTURE_SMOOTHNESS_NAME = "SmoothnessMap.png";
        private const string _TEXTURE_REFLECTION_NAME = "ReflectionMap.png";
        private const string _TEXTURE_EMISSION_NAME = "EmissionMap.png";
        private const string _TEXTURE_OPACITY_NAME = "OpacityMap.png";
        private const string _TEXTURE_PREFIX = "T_";

        private static readonly int _COLOR_MAP = Shader.PropertyToID("_ColorMap");
        private static readonly int _METALLIC_MAP = Shader.PropertyToID("_MetallicMap");
        private static readonly int _SMOOTHNESS_MAP = Shader.PropertyToID("_SmoothnessMap");
        private static readonly int _REFLECTION_MAP = Shader.PropertyToID("_ReflectionMap");
        private static readonly int _EMISSION_MAP = Shader.PropertyToID("_EmissionMap");
        private static readonly int _OPACITY_MAP = Shader.PropertyToID("_OpacityMap");
        private static Queue<Action> _callbackQueue = new Queue<Action>();
        private static bool _openWindowOnStart = true;
        private static string _pendingLoadFilePath;
        private readonly List<SidekickColorRow> _visibleColorRows = new List<SidekickColorRow>();

        private List<SidekickColorRow> _allColorRows = new List<SidekickColorRow>();
        private List<SidekickPart> _allParts;
        private List<SidekickSpecies> _allSpecies;
        private ObjectField _animationField;
        private FilterGroup _appliedPartFilters;
        private bool _applyingPreset = false;
        private bool _regenerateQueued = false;
        private bool _reapplyColorsOnRegen = false;
        private double _lastRegenRequestTime = 0;
        private const double _REGEN_DEBOUNCE_SECONDS = 0.2;
        private List<SidekickPart> _availablePartList;
        List<SidekickPartPreset> _availablePresets;
        private ObjectField _baseModelField;
        private Dictionary<string, Vector3> _blendShapeRigMovement = new Dictionary<string, Vector3>();
        private Dictionary<string, Quaternion> _blendShapeRigRotation = new Dictionary<string, Quaternion>();
        private ToolbarToggle _bodyPartsTab;
        private ToolbarToggle _bodyPresetTab;
        private ToolbarToggle _bodyShapeTab;
        private ScrollView _bodyShapeView;
        private float _bodySizeHeavyBlendValue;
        private float _bodySizeSkinnyBlendValue;
        private Slider _bodySizeSlider;
        private Slider _bodyTypeSlider;
        private VisualElement _colorSelectionRowView;
        private ToolbarToggle _colorSelectionTab;
        private ScrollView _colorSelectionView;
        private DropdownField _colorSetsDropdown;
        private bool _combineBodyBlendShapes = true;
        private Toggle _combineBodyBlendsToggle;
        private bool _combineFacialBlendShapes = true;
        private Toggle _combineFacialBlendsToggle;
        private bool _combineMeshes = true;
        private Toggle _combineMeshesToggle;
        private AnimatorController _currentAnimationController;
        private Dictionary<string, SidekickBodyShapePreset> _currentBodyPresetDictionary = new Dictionary<string, SidekickBodyShapePreset>();
        private Dictionary<CharacterPartType, SidekickPart> _currentCharacter = new Dictionary<CharacterPartType, SidekickPart>();
        private Dictionary<string, SidekickColorPreset> _currentColorSpeciesPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorOutfitsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorAttachmentsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorMaterialsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private Dictionary<string, SidekickColorPreset> _currentColorElementsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
        private SidekickColorSet _currentColorSet;
        private bool _currentGlobalLockStatus;
        private Material _currentMaterial;
        private Dictionary<CharacterPartType, string> _previousPartSelections;
        private ColorPartType _currentPartType;
        private Dictionary<string, SidekickPartPreset> _currentHeadPresetDictionary = new Dictionary<string, SidekickPartPreset>();
        private Dictionary<string, SidekickPartPreset> _currentUpperBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
        private Dictionary<string, SidekickPartPreset> _currentLowerBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
        private SidekickSpecies _currentSpecies;
        private TabView _currentTab;
        private Dictionary<ColorPartType, List<Vector2>> _currentUVDictionary = new Dictionary<ColorPartType, List<Vector2>>();
        private List<Vector2> _currentUVList = new List<Vector2>();
        private DatabaseManager _dbManager;
        private string _dbPath;
        private ToolbarToggle _decalSelectionTab;
        private ScrollView _decalSelectionView;
        private StyleSheet _editorStyle;
        private bool _forceBaseModel = false;
        private bool _showAllColourProperties = false;
        private float _bodyTypeBlendValue;
        private bool _loadingCharacter = false;
        private bool _loadingContent = true;
        private Image _loadingImage;
        private ObjectField _materialField;
        private float _musclesBlendValue;
        private Slider _musclesSlider;
        private GameObject _newModel;
        private VisualElement _newSetNameContainer;
        private ScrollView _optionSelectionView;
        private ToolbarToggle _optionTab;
        private Label _partCountLabel;
        private Dictionary<CharacterPartType, SkinnedMeshRenderer> _partDictionary;
        private Dictionary<CharacterPartType, Dictionary<string, string>> _partLibrary;
        private Dictionary<CharacterPartType, List<SidekickPart>> _allPartsLibrary;
        private Dictionary<string, List<string>> _partOutfitMap;
        private Dictionary<PopupField<string>, bool> _partLockMap;
        private Dictionary<CharacterPartType, PartTypeControls> _partSelectionDictionary;
        private Foldout _partsFoldout;
        private Dictionary<SidekickSpecies, List<string>> _partSpeciesMap;
        private Dictionary<SidekickPresetFilter, bool> _partPresetFilterToggleMap = new Dictionary<SidekickPresetFilter, bool>();
        private ScrollView _partView;
        private Dictionary<string, string> _presetDefaultValues = new Dictionary<string, string>();
        private readonly Dictionary<string, PresetRowControls> _presetRowControls = new Dictionary<string, PresetRowControls>();
        private VisualElement _presetPartContainer;
        private ScrollView _presetView;
        private Toggle _previewToggle;
        private bool _processingSpeciesChange = false;
        private bool _requiresWrap = false;
        private string _lastSelectedWrap;
        private VisualElement _root;
        private bool _showMissingPartsPopup = false;
        private SidekickRuntime _sidekickRuntime;
        private DropdownField _speciesField;
        private DropdownField _speciesPresetField;
        // ExitingEditMode doubles as the "no pending reload" state; the enum default (EnteredEditMode) would falsely
        // trigger a character reload when the window is first opened.
        private PlayModeStateChange _stateChange = PlayModeStateChange.ExitingEditMode;
        private bool _useAutoSaveAndLoad = false;
        private List<SidekickColorSet> _visibleColorSets = new List<SidekickColorSet>();

        // Undo support: the tool state is mirrored into a hidden ScriptableObject so the editor Undo system has
        // an object to track. Serialized fields survive domain reloads together with the native undo stack.
        [SerializeField]
        private SidekickUndoProxy _undoProxy;
        [SerializeField]
        private string _lastAppliedSnapshot;
        [SerializeField]
        private string _lastAppliedPresetSelections;
        private bool _isApplyingUndoState;
        private int _lastRecordedUndoGroup = -1;

        // Animation variables
        private Animator _currentAnimator;
        private string _defaultStateName = "Idle";
        private float _defaultClipDurationFallback = 2f;
        private float _blendDuration = 0.5f; // seconds
        private List<string> _otherStates = new List<string>();
        private string _currentState;
        private float _playbackTime;
        private float _stateDuration = 1f;
        private int _loopsRemaining = 0;
        private bool _playingDefault = true;
        private double _lastEditorTime;
        private Transform[] _animatorBones;
        private Dictionary<Transform, Vector3> _previousPosePos = new Dictionary<Transform, Vector3>();
        private Dictionary<Transform, Quaternion> _previousPoseRot = new Dictionary<Transform, Quaternion>();
        private float _blendElapsedTime = 0f;
        private bool _blending = false;

        /// <inheritdoc cref="Awake" />
        private void Awake()
        {
            InitializeEditorWindow();
        }

        /// <inheritdoc cref="OnDestroy" />
        private void OnDestroy()
        {
            // A window with no character state must not overwrite the snapshot saved when the character was last built.
            if (_useAutoSaveAndLoad && _currentCharacter.Count > 0)
            {
                SerializedCharacter savedCharacter = CreateSerializedCharacter(_OUTPUT_MODEL_NAME);
                Serializer serializer = new Serializer();
                string serializedCharacter = serializer.Serialize(savedCharacter);
                EditorPrefs.SetString(_AUTOSAVE_KEY, serializedCharacter);
            }

            if (_undoProxy != null)
            {
                // Remove the tool's entries from the editor undo stack; they would otherwise linger in the
                // Edit menu referencing a destroyed object.
                Undo.ClearUndo(_undoProxy);
                DestroyImmediate(_undoProxy);
                _undoProxy = null;
            }

            // ensures we release the file lock on the database
            _dbManager.CloseConnection();
        }

#if UNITY_EDITOR
        /// <inheritdoc cref="OnEnable" />
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += StateChange;
            EditorApplication.update += AnimationUpdate;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            EditorApplication.update -= AnimationUpdate;
            EditorApplication.update -= DebouncedRegenerationTick;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        /// <inheritdoc cref="Update" />
        private void Update()
        {
            // _loadingImage is only created once the UI is built; Update() can tick before that (notably in Unity 6),
            // so guard against it still being null while _loadingContent defaults to true.
            if (_loadingContent && _loadingImage != null)
            {
#if UNITY_6000_3_OR_NEWER
                float angle = _loadingImage.resolvedStyle.rotate.angle.value + 0.5f * Time.deltaTime;
                _loadingImage.style.rotate = new Rotate(angle);
#else
                Vector3 rotation = _loadingImage.transform.rotation.eulerAngles;
                rotation += new Vector3(0, 0, 0.5f * Time.deltaTime);
                _loadingImage.transform.rotation = Quaternion.Euler(rotation);
#endif
            }

            while (_callbackQueue.Count > 0)
            {
                Action action = null;
                lock (_callbackQueue)
                {
                    if (_callbackQueue.Count > 0)
                    {
                        action = _callbackQueue.Dequeue();
                    }
                }
                action?.Invoke();
            }
        }

        /// <summary>
        ///    Sets up the animation controllers for playing during runtime.
        /// </summary>
        /// <returns>True if it is set up; otherwise false.</returns>
        private bool SetupAnimationControllers()
        {
            _loopsRemaining = 0;

            _currentAnimator = _newModel.GetComponent<Animator>();
            if (_currentAnimator != null)
            {
                _currentAnimationController = _currentAnimator.runtimeAnimatorController as AnimatorController;
                _sidekickRuntime.CurrentAnimationController = _currentAnimationController;
            }
            else
            {
                return false;
            }

            _animatorBones = _currentAnimator.GetComponentsInChildren<Transform>();
            CollectOtherStates();

            if (string.IsNullOrEmpty(_currentState))
            {
                StartDefaultState();
            }
            else
            {
                SetState(_currentState);
            }

            _lastEditorTime = EditorApplication.timeSinceStartup;
            return true;
        }

        /// <summary>
        ///    Plays the animation during edit time.
        /// </summary>
        private void AnimationUpdate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (_animationField == null || _animationField.value == null)
            {
                return;
            }

            if (_currentAnimator == null || _currentAnimationController == null)
            {
                if (!SetupAnimationControllers())
                {
                    return;
                }
            }

            double currentTime = EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentTime - _lastEditorTime);
            _lastEditorTime = currentTime;

            _playbackTime += deltaTime;
            float normalizedTime = (_playbackTime % _stateDuration) / _stateDuration;

            _currentAnimator.Play(_currentState, 0, normalizedTime);
            _currentAnimator.Update(0f);

            // Blend from previous pose (if blending)
            if (_blending)
            {
                _blendElapsedTime += deltaTime;
                float t = Mathf.Clamp01(_blendElapsedTime / _blendDuration);

                foreach (var bone in _animatorBones)
                {
                    if (bone == null)
                    {
                        continue;
                    }

                    if (_previousPosePos.TryGetValue(bone, out Vector3 prevPos))
                    {
                        bone.localPosition = Vector3.Lerp(prevPos, bone.localPosition, t);
                    }

                    if (_previousPoseRot.TryGetValue(bone, out Quaternion prevRot))
                    {
                        bone.localRotation = Quaternion.Slerp(prevRot, bone.localRotation, t);
                    }
                }

                if (t >= +_blendDuration)
                {
                    _blending = false;
                }
            }

            SceneView.RepaintAll();

            if (_playbackTime >= _stateDuration)
            {
                _playbackTime = 0f;
                _loopsRemaining--;

                if (_loopsRemaining <= 0)
                {
                    if (!_playingDefault)
                    {
                        StartDefaultState();
                    }
                    else
                    {
                        _loopsRemaining = Int32.MaxValue;
                    }
                }
            }
        }

        /// <summary>
        ///    Collect other states from the animation contoller.
        /// </summary>
        private void CollectOtherStates()
        {
            _otherStates.Clear();
            if (_currentAnimationController == null)
            {
                return;
            }

            foreach (var layer in _currentAnimationController.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    string stateName = state.state.name;
                    if (stateName != _defaultStateName)
                    {
                        _otherStates.Add(stateName);
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void StartDefaultState()
        {
            SetState(_defaultStateName);
        }

        /// <summary>
        ///     Sets the state of the animation controller.
        /// </summary>
        /// <param name="stateName">The name of the state to set.</param>
        private void SetState(string stateName)
        {
            if (_currentAnimationController == null || _currentAnimator == null)
            {
                return;
            }

            CacheCurrentPose();

            _currentState = stateName;
            _playbackTime = 0f;
            _blendElapsedTime = 0f;
            _blending = true;
            _loopsRemaining = stateName == _defaultStateName ? Int32.MaxValue : 1;
            _playingDefault = stateName == _defaultStateName;

            var sm = _currentAnimationController.layers[0].stateMachine;
            foreach (var state in sm.states)
            {
                if (state.state.name == stateName && state.state.motion is AnimationClip clip)
                {
                 _stateDuration = Mathf.Max(clip.length, 0.01f);
                 goto Found;
                }
            }

            _stateDuration = _defaultClipDurationFallback;

            Found:
            _currentAnimator.Play(stateName, 0, 0f);
            _currentAnimator.Update(0f);
        }

        /// <summary>
        ///     Caches the current animation pose
        /// </summary>
        private void CacheCurrentPose()
        {
            _previousPosePos.Clear();
            _previousPoseRot.Clear();

            foreach (var bone in _animatorBones)
            {
                if (bone == null) continue;
                _previousPosePos[bone] = bone.localPosition;
                _previousPoseRot[bone] = bone.localRotation;
            }
        }

        /// <summary>
        ///     Processes the callback from the play mode state change.
        /// </summary>
        /// <param name="stateChange">The current PlayModeStateChange</param>
        private void StateChange(PlayModeStateChange stateChange)
        {
            // The character and tool selections always persist across play sessions, independent of the auto save/load
            // option. A window with no character state (e.g. freshly rebuilt by a recompile) must not overwrite the
            // snapshot saved when the character was last built. Color rows count as state on their own so that colors
            // detected from a scene character survive a play session even though no parts are selected.
            if (_currentSpecies != null
                && (_currentCharacter.Count > 0 || _allColorRows.Count > 0)
                && (stateChange == PlayModeStateChange.ExitingEditMode || stateChange == PlayModeStateChange.ExitingPlayMode))
            {
                SerializedCharacter savedCharacter = CreateSerializedCharacter(_OUTPUT_MODEL_NAME);
                Serializer serializer = new Serializer();
                string serializedCharacter = serializer.Serialize(savedCharacter);
                EditorPrefs.SetString(_AUTOSAVE_KEY, serializedCharacter);
            }

            if (stateChange == PlayModeStateChange.EnteredEditMode || stateChange == PlayModeStateChange.EnteredPlayMode)
            {
                _stateChange = stateChange;
            }
        }
#endif

        /// <inheritdoc cref="CreateGUI" />
        public async void CreateGUI()
        {
            _loadingContent = true;
            InitializeEditorWindow();
            _root = rootVisualElement;
            _root.Clear();
            if (_editorStyle != null)
            {
                _root.styleSheets.Add(_editorStyle);
            }

            InitializeDatabase();
            // if we still can't connect, something's gone wrong, don't keep building the GUI
            if (_dbManager?.GetCurrentDbConnection() == null)
            {
                return;
            }

            // Maintains a linking to the model if the editor window is closed and re-opened.
            _newModel = GameObject.Find(_OUTPUT_MODEL_NAME);

            _previousPartSelections = new Dictionary<CharacterPartType, string>();
            foreach (CharacterPartType type in Enum.GetValues(typeof(CharacterPartType)))
            {
                _previousPartSelections.Add(type, "None");
            }

            _partCountLabel = new Label("")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft
                }
            };

            Image bannerImage = new Image
            {
                image = (Texture2D) Resources.Load("UI/T_SidekickTitle"),
                scaleMode = ScaleMode.ScaleToFit,
            };
            VisualElement bannerLayout = new VisualElement
            {
                style =
                {
                    backgroundColor = new Color(209f/256, 34f/256, 51f/256),
                    minHeight = 150,
                    paddingBottom = 5,
                    paddingTop = 5,
                }
            };
            bannerLayout.Add(bannerImage);

            if (File.Exists(_VERSION_FILE_PATH))
            {
                Label versionLabel = new Label("v" + File.ReadAllText(_VERSION_FILE_PATH).Trim())
                {
                    style =
                    {
                        position = new StyleEnum<Position>(Position.Absolute),
                        bottom = 4,
                        right = 6,
                        color = Color.white,
                        fontSize = 10
                    }
                };
                bannerLayout.Add(versionLabel);
            }

            _root.Add(bannerLayout);

            _presetView = new ScrollView(ScrollViewMode.Vertical);
            _partView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };
            _bodyShapeView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None,
                    paddingLeft = 15,
                    paddingRight = 15
                }
            };

            _colorSelectionView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None,
                    paddingTop = 5,
                    paddingLeft = 15,
                    paddingRight = 15
                }
            };

            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionView = new ScrollView(ScrollViewMode.Vertical)
            // {
            //     style =
            //     {
            //         display = DisplayStyle.None
            //     }
            // };

            _optionSelectionView = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    display = DisplayStyle.None
                }
            };

            // TODO: Replace this tabbed menu code with TabView when 2023 LTS in the minimum supported version.
            Toolbar tabBar = new Toolbar
            {
                style =
                {
                    width = Length.Percent(100)
                }
            };

            _bodyPresetTab = new ToolbarToggle
            {
                text = "Presets",
                tooltip = "Create a character using preset combinations of parts, body types and colors"
            };

            _bodyPartsTab = new ToolbarToggle
            {
                text = "Parts",
                tooltip = "Edit individual character parts on your character"
            };

            _bodyShapeTab = new ToolbarToggle
            {
                text = "Body",
                tooltip = "Edit the body type, size, musculature of your character"
            };

            _colorSelectionTab = new ToolbarToggle
            {
                text = "Colors",
                tooltip = "Edit individual colors of your character"
            };

            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionTab = new ToolbarToggle
            // {
            //     text = "Decals",
            //     tooltip = "Edit the decals applied to your character"
            // };

            _optionTab = new ToolbarToggle
            {
                text = "Options",
                tooltip = "Change the options of the tool"
            };

            tabBar.Add(_bodyPresetTab);
            tabBar.Add(_bodyPartsTab);
            tabBar.Add(_bodyShapeTab);
            tabBar.Add(_colorSelectionTab);
            // TODO: Uncomment when Decals added to the system.
            // tabBar.Add(_decalSelectionTab);
            tabBar.Add(_optionTab);
            _root.Add(tabBar);

            _bodyPresetTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Preset && _bodyPresetTab.value)
                    {
                        SwitchToTab(TabView.Preset);
                    }
                }
            );

            _bodyPartsTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Parts && _bodyPartsTab.value)
                    {
                        SwitchToTab(TabView.Parts);
                    }
                }
            );

            _bodyShapeTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Body && _bodyShapeTab.value)
                    {
                        AddBodyShapeTabContent(_bodyShapeView);
                        SwitchToTab(TabView.Body);
                    }
                }
            );

            _colorSelectionTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Colors && _colorSelectionTab.value)
                    {
                        // always re-populate the color rows with the latest when switching tabs
                        if (_allColorRows.Count == 0)
                        {
                            PopulateColorRowsFromTextures();
                        }
                        else
                        {
                            PopulatePartColorRows();
                            RefreshVisibleColorRows();
                        }

                        SwitchToTab(TabView.Colors);
                    }
                }
            );

            // TODO: Uncomment when Decals added to the system.
            // _decalSelectionTab.RegisterValueChangedCallback(
            //     delegate
            //     {
            //         if (_currentTab != TabView.Decals && _decalSelectionTab.value)
            //         {
            //             SwitchToTab(TabView.Decals);
            //         }
            //     }
            // );

            _optionTab.RegisterValueChangedCallback(
                delegate
                {
                    if (_currentTab != TabView.Options && _optionTab.value)
                    {
                        SwitchToTab(TabView.Options);
                    }
                    // If currently on this tab, and button is toggled to "off", toggle back to "on"
                    else if (_currentTab == TabView.Options && !_optionTab.value)
                    {
                        _optionTab.value = true;
                    }
                }
            );

            _root.Add(_presetView);
            _root.Add(_partView);
            _root.Add(_bodyShapeView);
            _root.Add(_colorSelectionView);
            // TODO: Uncomment when Decals added to the system.
            // root.Add(_decalSelectionView);
            _root.Add(_optionSelectionView);

            AddOptionsTabContent(_optionSelectionView);

            // set the default colour set (this will change on various input triggers, but we need it to not be null)
            _currentColorSet = SidekickColorSet.GetDefault(_dbManager);

            _partDictionary = new Dictionary<CharacterPartType, SkinnedMeshRenderer>();
            _currentCharacter = new Dictionary<CharacterPartType, SidekickPart>();

            _sidekickRuntime = new SidekickRuntime((GameObject) _baseModelField.value, (Material) _materialField.value, _currentAnimationController, _dbManager);
            _sidekickRuntime.ForceAssignedBaseModel = _forceBaseModel;

            Label loadingLabel = new Label
            {
                text = "Loading Content..",
                style =
                {
                    fontSize = 20,
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                    marginTop = 20
                }
            };

            _presetView.Add(loadingLabel);

            _loadingImage = new Image
            {
                image = (Texture2D) Resources.Load("UI/T_LoadingCircle"),
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 28,
                    height = 28,
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    alignSelf = new StyleEnum<Align>(Align.Center)
                }
            };

            _presetView.Add(_loadingImage);

            _bodyPresetTab.value = true;
            SwitchToTab(TabView.Preset);

            VisualElement saveLoadButtons = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    alignContent = new StyleEnum<Align>(Align.FlexStart),
                    alignItems = new StyleEnum<Align>(Align.FlexStart),
                    flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
                    justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween),
                    minHeight = 30,
                    marginTop = 20
                }
            };

            _root.Add(saveLoadButtons);

            Button loadCharacterButton = new Button(LoadCharacter)
            {
                text = "Load Character",
                style =
                {
                    minHeight = 30,
                    width = Length.Percent(48)
                }
            };
            saveLoadButtons.Add(loadCharacterButton);

            Button saveCharacterButton = new Button(SaveCharacter)
            {
                text = "Save Character",
                style =
                {
                    minHeight = 30,
                    width = Length.Percent(48)
                }
            };

            saveLoadButtons.Add(saveCharacterButton);

            Button createCharacterButton = new Button(CreateCharacterPrefab)
            {
                text = "Export Character as FBX",
                style =
                {
                    minHeight = 50,
                    marginTop = 5
                }
            };
            _root.Add(createCharacterButton);

            // Populate the data in an async process, if not already populated
            try
            {
                await Task.Run(
                    async () =>
                    {
                        await SidekickRuntime.PopulateToolData(_sidekickRuntime);
                        _callbackQueue.Enqueue(AddAllTabContent);
                        if (_stateChange == PlayModeStateChange.EnteredEditMode
                            || _stateChange == PlayModeStateChange.EnteredPlayMode)
                        {
                            _callbackQueue.Enqueue(ReloadCharacterFromStateChange);
                        }
                    }
                );
            }
            catch
            {
                Debug.LogWarning("Failed to load tool data. Please try again.\nPlease note that data loading may take some time.");
            }
        }

        /// <summary>
        ///     Reloads the character based on auto saved character
        /// </summary>
        private void ReloadCharacterFromStateChange()
        {
            string serializedCharacterString = EditorPrefs.GetString(_AUTOSAVE_KEY, null);
            SerializedCharacter serializedCharacter = null;

            if (!string.IsNullOrEmpty(serializedCharacterString))
            {
                Deserializer deserializer = new Deserializer();
                serializedCharacter = deserializer.Deserialize<SerializedCharacter>(serializedCharacterString);
            }

            bool snapshotHasParts = serializedCharacter?.Parts != null && serializedCharacter.Parts.Count > 0;

            // Only clear the scene character when a full snapshot is about to rebuild it; with nothing to rebuild
            // from, destroying it would throw away the user's only copy of the character.
            if (_stateChange == PlayModeStateChange.EnteredEditMode && snapshotHasParts)
            {
                GameObject existingModel = GameObject.Find(_OUTPUT_MODEL_NAME);
                while (existingModel != null)
                {
                    GameObject.DestroyImmediate(existingModel);
                    existingModel = GameObject.Find(_OUTPUT_MODEL_NAME);
                }
            }

            if (snapshotHasParts)
            {
                LoadSerializedCharacter(serializedCharacter, _showAllColourProperties);
            }
            else
            {
                // No parts to rebuild (e.g. only scene detection had run before play was pressed). Keep the scene
                // character, restore the part and UV data from it, then restore the colors serialized before play.
                DetectSceneCharacter();

                if (serializedCharacter?.ColorSet != null
                    && serializedCharacter.ColorRows != null
                    && serializedCharacter.ColorRows.Count > 0)
                {
                    LoadColorSet(serializedCharacter);
                    UpdateColorTabContent();
                }
            }

            _stateChange = PlayModeStateChange.ExitingEditMode;
        }

        /// <summary>
        ///     Adds the UI content to all tabs (excluding Options tab as it is already populated)
        /// </summary>
        private void AddAllTabContent()
        {
            if (_sidekickRuntime.PartCount < 15)
            {
                if (EditorUtility.DisplayDialog(
                        "Unable to strat Sidekicks Tool",
                        "There are not enough Sidekicks parts currently in your project. The tool is unable to start without the requisite parts",
                        "Ok"
                    ))
                {
                    Close();
                }

            }
            else {
                _allPartsLibrary = _sidekickRuntime.AllPartsLibrary;
                _allParts = SidekickPart.GetAll(_dbManager);

                _allSpecies = SidekickSpecies.GetAll(_dbManager);
                _currentSpecies = _allSpecies[0];
                _sidekickRuntime.CurrentSpecies = _currentSpecies;

                _partCountLabel.text = _sidekickRuntime.PartCount + _PART_COUNT_BODY;
                _partOutfitMap = _sidekickRuntime.PartOutfitMap;

                AddBodyShapeTabContent(_bodyShapeView);
                AddColorTabContent(_colorSelectionView);
                // TODO: Uncomment when Decals added to the system.
                // _decalSelectionView.Add(new Label("Decal selections will go here."));

                UpdatePartUVData();

                PopulatePartUI();
                PopulatePresetUI();

                if (_currentSpecies == null)
                {
                    _currentSpecies = _currentSpecies = _allSpecies.FirstOrDefault(species => species.Name == _speciesField.value);
                }

                bool restoredSavedCharacter = false;
                if (_useAutoSaveAndLoad)
                {
                    string serializedCharacterString = EditorPrefs.GetString(_AUTOSAVE_KEY, null);

                    if (!string.IsNullOrEmpty(serializedCharacterString))
                    {
                        // The attempt counts as a restore even if it fails, so scene detection below
                        // never runs on a half-loaded state.
                        restoredSavedCharacter = true;

                        try
                        {
                            Deserializer deserializer = new Deserializer();
                            SerializedCharacter serializedCharacter = deserializer.Deserialize<SerializedCharacter>(serializedCharacterString);
                            LoadSerializedCharacter(serializedCharacter, _showAllColourProperties);
                        }
                        catch (Exception ex)
                        {
                            EditorUtility.DisplayDialog(
                                "Something Went Wrong",
                                "Unable to load saved character.",
                                "OK"
                            );
                            Debug.LogWarning(ex);
                            _useAutoSaveAndLoad = false;
                            EditorPrefs.SetBool(_AUTOSAVE_STATE, _useAutoSaveAndLoad);
                        }
                    }
                }
                _loadingContent = false;

                bool pendingFileLoad = !string.IsNullOrEmpty(_pendingLoadFilePath);
                if (pendingFileLoad)
                {
                    string pendingLoadPath = _pendingLoadFilePath;
                    _pendingLoadFilePath = null;
                    LoadCharacterFromPath(pendingLoadPath);
                }

                // With no saved/pending character taking priority, pick up a character already in the
                // scene so the tool reflects its parts and colors. The last two guards keep this
                // to a fresh session; a mid-session rebuild (e.g. Update Part Library) must not replace
                // in-memory color/UV state with data re-detected from the tool's own combined preview.
                if (!restoredSavedCharacter
                    && !pendingFileLoad
                    && _stateChange == PlayModeStateChange.ExitingEditMode
                    && _allColorRows.Count == 0
                    && (_currentUVList == null || _currentUVList.Count == 0))
                {
                    DetectSceneCharacter();
                }

                // Seed the undo baseline for the fresh-empty-window path; every other startup path has scheduled
                // a rebuild whose CommitUndoState seeds it (this call no-ops mid-batch).
                CommitUndoState();
            }
        }

        /// <summary>
        ///     Initializes all the database setup, and provides the status label reference for users
        /// </summary>
        private void InitializeDatabase()
        {
            // TODO: always reinstantiate instead?
            if (_dbManager != null)
            {
                return;
            }

            _dbManager = new DatabaseManager();
            _dbManager.GetDbConnection(true);
        }

        /// <summary>
        ///     Initializes the editor window.
        /// </summary>
        private void InitializeEditorWindow()
        {
            _editorStyle = Resources.Load<StyleSheet>("Styles/EditorStyles");
            _openWindowOnStart = EditorPrefs.GetBool(_AUTO_OPEN_STATE, true);
            _useAutoSaveAndLoad = EditorPrefs.GetBool(_AUTOSAVE_STATE, false);
            _showMissingPartsPopup = EditorPrefs.GetBool(_AUTOSAVE_MISSING_PARTS, false);
            _combineMeshes = EditorPrefs.GetBool(_COMBINE_MESHES_STATE, true);
            _combineBodyBlendShapes = EditorPrefs.GetBool(_COMBINE_BODY_BLENDS_STATE, true);
            _combineFacialBlendShapes = EditorPrefs.GetBool(_COMBINE_FACIAL_BLENDS_STATE, true);
            _forceBaseModel = EditorPrefs.GetBool(_FORCE_BASE_MODEL_STATE, false);
        }

        /// <summary>
        ///     Adds the contents and change listeners for the body shape tab.
        /// </summary>
        /// <param name="view">The tabview to add the content to.</param>
        private void AddBodyShapeTabContent(ScrollView view)
        {
            view.Clear();

            _bodyTypeSlider = new Slider("Body Type", -100, 100)
            {
                value = _bodyTypeBlendValue,
                style =
                {
                    maxWidth = new StyleLength(StyleKeyword.Auto)
                },
                showInputField = true,
                tooltip = "Blend the body type of the character between masculine and feminine"
            };

            VisualElement bodyTypeLabels = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    paddingLeft = 155
                }
            };

            Label labelMasculine = new Label("Masculine")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = 155
                }
            };
            bodyTypeLabels.Add(labelMasculine);

            Label labelFeminine = new Label("Feminine")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = 58
                }
            };
            bodyTypeLabels.Add(labelFeminine);

            _bodySizeSlider = new Slider("Body Size", -100, 100)
            {
                value = _bodySizeSkinnyBlendValue > 0 ? -_bodySizeSkinnyBlendValue : _bodySizeHeavyBlendValue,
                style =
                {
                    maxWidth = new StyleLength(StyleKeyword.Auto),
                    marginTop = 30
                },
                showInputField = true,
                tooltip = "Blend the body size of the character between slim and heavy"
            };

            VisualElement bodySizeLabels = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    paddingLeft = 155
                }
            };

            Label labelSlim = new Label("Slim")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = 155
                }
            };
            bodySizeLabels.Add(labelSlim);

            Label labelHeavy = new Label("Heavy")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = 58
                }
            };
            bodySizeLabels.Add(labelHeavy);

            _musclesSlider = new Slider("Musculature", -100, 100)
            {
                value = _musclesBlendValue,
                style =
                {
                    maxWidth = new StyleLength(StyleKeyword.Auto),
                    marginTop = 30
                },
                showInputField = true,
                tooltip = "Blend the musculature of the character between lean and muscular"
            };

            VisualElement muscleLabels = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    width = Length.Percent(100),
                    height = 20,
                    paddingLeft = 155
                }
            };

            Label labelLean = new Label("Lean")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    left = 155
                }
            };
            muscleLabels.Add(labelLean);

            Label labelBulk = new Label("Muscular")
            {
                style =
                {
                    position = new StyleEnum<Position>(Position.Absolute),
                    right = 58
                }
            };
            muscleLabels.Add(labelBulk);

            view.Add(_bodyTypeSlider);
            view.Add(bodyTypeLabels);
            view.Add(_bodySizeSlider);
            view.Add(bodySizeLabels);
            view.Add(_musclesSlider);
            view.Add(muscleLabels);

            _bodyTypeSlider.RegisterValueChangedCallback(
                evt =>
                {
                    _bodyTypeBlendValue = evt.newValue;
                    _sidekickRuntime.BodyTypeBlendValue = evt.newValue;

                    bool wrapChanged = SyncWrapToBodyType();

                    // During a batch the single debounced rebuild creates/updates the model, so don't build early here.
                    if (_newModel == null && !_applyingPreset)
                    {
                        RegeneratePreviewCharacter(false, true);
                        UpdatePartUVData();
                    }
                    else if (wrapChanged && !_applyingPreset && _previewToggle.value)
                    {
                        // The wrap mesh was added or removed; the in-place blend shape update below cannot do that,
                        // so rebuild once at the threshold crossing.
                        RegeneratePreviewCharacter(false, true);
                        UpdatePartUVData();
                    }

                    // During a batch operation (randomize / preset / load) the single debounced rebuild
                    // re-applies blend shapes, so skip the in-place update to avoid a separate visible stage.
                    if (!_applyingPreset)
                    {
                        _sidekickRuntime.UpdateBlendShapes(_newModel);
                        _sidekickRuntime.ProcessRigMovementOnBlendShapeChange(SidekickBlendShapeRigMovement.GetAllForProcessing(_dbManager));
                        _sidekickRuntime.ProcessBoneMovement(_newModel);

                        // Sliders are not undoable themselves, but committing here keeps a later part/color undo
                        // from silently reverting slider changes made since the last commit.
                        CommitUndoState();
                    }
                }
            );

            _bodySizeSlider.RegisterValueChangedCallback(
                evt =>
                {
                    float newValue = evt.newValue;
                    if (newValue > 0)
                    {
                        _bodySizeHeavyBlendValue = newValue;
                        _bodySizeSkinnyBlendValue = 0;
                        _sidekickRuntime.BodySizeHeavyBlendValue = newValue;
                        _sidekickRuntime.BodySizeSkinnyBlendValue = 0;
                    }
                    else if (newValue < 0)
                    {
                        _bodySizeHeavyBlendValue = 0;
                        _bodySizeSkinnyBlendValue = -newValue;
                        _sidekickRuntime.BodySizeHeavyBlendValue = 0;
                        _sidekickRuntime.BodySizeSkinnyBlendValue = -newValue;
                    }
                    else
                    {
                        _bodySizeHeavyBlendValue = 0;
                        _bodySizeSkinnyBlendValue = 0;
                        _sidekickRuntime.BodySizeHeavyBlendValue = 0;
                        _sidekickRuntime.BodySizeSkinnyBlendValue = 0;
                    }

                    // During a batch the single debounced rebuild creates/updates the model, so don't build early here.
                    if (_newModel == null && !_applyingPreset)
                    {
                        RegeneratePreviewCharacter(false, true);
                        UpdatePartUVData();
                    }

                    // During a batch operation (randomize / preset / load) the single debounced rebuild
                    // re-applies blend shapes, so skip the in-place update to avoid a separate visible stage.
                    if (!_applyingPreset)
                    {
                        _sidekickRuntime.UpdateBlendShapes(_newModel);
                        _sidekickRuntime.ProcessRigMovementOnBlendShapeChange(SidekickBlendShapeRigMovement.GetAllForProcessing(_dbManager));
                        _sidekickRuntime.ProcessBoneMovement(_newModel);

                        // Sliders are not undoable themselves, but committing here keeps a later part/color undo
                        // from silently reverting slider changes made since the last commit.
                        CommitUndoState();
                    }
                }
            );

            _musclesSlider.RegisterValueChangedCallback(
                evt =>
                {
                    _musclesBlendValue = evt.newValue;
                    _sidekickRuntime.MusclesBlendValue = evt.newValue;

                    // During a batch the single debounced rebuild creates/updates the model, so don't build early here.
                    if (_newModel == null && !_applyingPreset)
                    {
                        RegeneratePreviewCharacter(false, true);
                        UpdatePartUVData();
                    }

                    // During a batch operation (randomize / preset / load) the single debounced rebuild
                    // re-applies blend shapes, so skip the in-place update to avoid a separate visible stage.
                    if (!_applyingPreset)
                    {
                        _sidekickRuntime.UpdateBlendShapes(_newModel);
                        _sidekickRuntime.ProcessRigMovementOnBlendShapeChange(SidekickBlendShapeRigMovement.GetAllForProcessing(_dbManager));
                        _sidekickRuntime.ProcessBoneMovement(_newModel);

                        // Sliders are not undoable themselves, but committing here keeps a later part/color undo
                        // from silently reverting slider changes made since the last commit.
                        CommitUndoState();
                    }
                }
            );
        }

        /// <summary>
        ///     Adds the content to the Color tab.
        /// </summary>
        /// <param name="view">The view to add the content to.</param>
        private void AddColorTabContent(ScrollView view)
        {
            _colorSetsDropdown = new DropdownField
            {
                style =
                {
                    maxWidth = Length.Percent(65)
                }
            };
            ScrollableDropdown.Apply(_colorSetsDropdown);

            Label filterPartsLabel = new Label("Filter - Parts")
            {
                tooltip = "Filter the displayed colors to focus on specific areas of the character and reduce the number of properties"
            };
            view.Add(filterPartsLabel);
            DropdownField partTypeDropdown = new DropdownField();
            ScrollableDropdown.Apply(partTypeDropdown);
            string[] colorPartTypes = Enum.GetNames(typeof(ColorPartType));
            // Enum names can't have spaces so we add in the space manually for display.
            for (int i = 0; i < colorPartTypes.Length; i++)
            {
                colorPartTypes[i] = StringUtils.AddSpacesBeforeCapitalLetters(colorPartTypes[i]);
            }

            partTypeDropdown.choices = colorPartTypes.ToList();
            partTypeDropdown.value = colorPartTypes[0];
            view.Add(partTypeDropdown);

            partTypeDropdown.RegisterValueChangedCallback(
                (evt) =>
                {
                    if (Enum.TryParse(typeof(ColorPartType), evt.newValue.Replace(" ", ""), out object newType))
                    {
                        _currentPartType = (ColorPartType) newType;
                        _colorSetsDropdown.value = "Custom";
                    }

                    PopulatePartColorRows();
                    RefreshVisibleColorRows();
                }
            );

            _currentPartType = ColorPartType.AllParts;

            // TODO: Hidden due to early access, enable when feature complete
            // Label colorSetsLabel = new Label("Color Sets");
            // view.Add(colorSetsLabel);
            //
            // VisualElement colorSetsRow = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
            //         width = Length.Percent(100),
            //         alignContent = new StyleEnum<Align>(Align.FlexStart),
            //         alignItems = new StyleEnum<Align>(Align.FlexStart),
            //         flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
            //         justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween),
            //         marginBottom = 10
            //     }
            // };

            UpdateVisibleColorSets();

            // TODO: Hidden due to early access, enable when feature complete
            // colorSetsRow.Add(_colorSetsDropdown);
            //
            // _colorSetsDropdown.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         if (evt.newValue != "Custom")
            //         {
            //             SidekickColorSet newSet = _visibleColorSets.First(set => set.Name == evt.newValue);
            //             List<SidekickColorRow> newRows = SidekickColorRow.GetAllBySet(_dbManager, newSet);
            //             if (_currentPartType == ColorPartType.AllParts && _allColorRows.All(row => row.IsLocked == false))
            //             {
            //                 _currentColorSet = newSet;
            //                 _allColorRows = newRows;
            //                 if (_allColorRows.Count == 0)
            //                 {
            //                     PopulateColorRowsFromTextures();
            //                 }
            //                 else
            //                 {
            //                     PopulatePartColorRows();
            //                     RefreshVisibleColorRows();
            //                 }
            //             }
            //             else
            //             {
            //                 foreach (SidekickColorRow row in _visibleColorRows)
            //                 {
            //                     SidekickColorRow newRow = newRows.FirstOrDefault(r => r.ColorProperty.ID == row.ColorProperty.ID);
            //                     if (newRow != null)
            //                     {
            //                         row.NiceColor = newRow.NiceColor;
            //                         row.NiceMetallic = newRow.NiceMetallic;
            //                         row.NiceSmoothness = newRow.NiceSmoothness;
            //                         row.NiceReflection = newRow.NiceReflection;
            //                         row.NiceEmission = newRow.NiceEmission;
            //                         row.NiceOpacity = newRow.NiceOpacity;
            //                     }
            //                 }
            //
            //                 RefreshVisibleColorRows();
            //             }
            //
            //             UpdateAllVisibleColors();
            //         }
            //     }
            // );
            //
            // Button previousSetButton = new Button(
            //     () =>
            //     {
            //         if (_colorSetsDropdown.index > 0)
            //         {
            //             _colorSetsDropdown.index -= 1;
            //         }
            //     }
            // )
            // {
            //     tooltip = "Previous Color Set"
            // };
            //
            // previousSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("tab_prev", "|Previous Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(previousSetButton);
            // Button nextSetButton = new Button(
            //     () =>
            //     {
            //         if (_colorSetsDropdown.index < _colorSetsDropdown.choices.Count - 1)
            //         {
            //             _colorSetsDropdown.index += 1;
            //         }
            //     }
            // )
            // {
            //     tooltip = "Next Color Set"
            // };
            //
            // nextSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("tab_next", "|Next Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(nextSetButton);
            // Button resetSetButton = new Button(ResetColorSet)
            // {
            //     tooltip = "Reset Color Set from Disk"
            // };
            // resetSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("Refresh", "|Reset Color Set From Disk").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(resetSetButton);
            // Button newSetButton = new Button(ShowCreateNewColorSet)
            // {
            //     tooltip = "Create New Color Set"
            // };
            // newSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("Toolbar Plus", "|Create New Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(newSetButton);
            // Button deleteSetButton = new Button(DeleteColorSet)
            // {
            //     tooltip = "Delete Color Set"
            // };
            // deleteSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("close", "|Delete Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(deleteSetButton);
            // Button saveSetButton = new Button(SaveColorSet)
            // {
            //     tooltip = "Save Color Set"
            // };
            // saveSetButton.Add(
            //     new Image
            //     {
            //         image = EditorGUIUtility.IconContent("SaveAs", "|Save Color Set").image,
            //         scaleMode = ScaleMode.ScaleToFit
            //     }
            // );
            //
            // colorSetsRow.Add(saveSetButton);
            // view.Add(colorSetsRow);
            //
            // _newSetNameContainer = new VisualElement
            // {
            //     style =
            //     {
            //         flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
            //         width = Length.Percent(100),
            //         alignContent = new StyleEnum<Align>(Align.FlexStart),
            //         alignItems = new StyleEnum<Align>(Align.FlexStart),
            //         flexWrap = new StyleEnum<Wrap>(Wrap.Wrap),
            //         marginBottom = 10,
            //         display = DisplayStyle.None
            //     }
            // };
            //
            // TextField newNameField = new TextField("New Set Name")
            // {
            //     style =
            //     {
            //         minWidth = Length.Percent(70)
            //     }
            // };
            // _newSetNameContainer.Add(newNameField);
            //
            // Button newSetCreateButton = new Button(
            //     () =>
            //     {
            //         CreateNewColorSet(newNameField.value);
            //         List<string> choices = _colorSetsDropdown.choices;
            //         choices.Add(newNameField.value);
            //         _colorSetsDropdown.choices = choices;
            //         _colorSetsDropdown.value = newNameField.value;
            //         _newSetNameContainer.style.display = DisplayStyle.None;
            //     }
            // )
            // {
            //     text = "Create Set"
            // };
            //
            // newNameField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         newSetCreateButton.SetEnabled(!SidekickColorSet.DoesNameExist(_dbManager, evt.newValue));
            //     }
            // );
            //
            // _newSetNameContainer.Add(newSetCreateButton);
            // view.Add(_newSetNameContainer);
            //
            // VisualElement allRow = new VisualElement();
            // allRow.AddToClassList("colorSelectionRow");
            //
            // Label allItemsLabel = new Label("All");
            // allItemsLabel.AddToClassList("colorSelectionRowLabel");
            // allRow.Add(allItemsLabel);
            //
            // VisualElement allRowContent = new VisualElement();
            // allRowContent.AddToClassList("colorSelectionRowContent");
            // allRow.Add(allRowContent);
            //
            // Button btnAllLock = new Button
            // {
            //     style =
            //     {
            //         left = 0
            //     }
            // };
            // allRowContent.Add(btnAllLock);
            // Image lockButtonImage = new Image
            // {
            //     image = _currentGlobalLockStatus
            //         ? EditorGUIUtility.IconContent("Locked").image
            //         : EditorGUIUtility.IconContent("Unlocked").image,
            //     scaleMode = ScaleMode.ScaleToFit
            // };
            //
            // btnAllLock.Add(lockButtonImage);
            //
            // btnAllLock.clickable.clicked += () =>
            // {
            //     foreach (SidekickColorRow colorRow in _visibleColorRows)
            //     {
            //         colorRow.IsLocked = !_currentGlobalLockStatus;
            //         if (colorRow.ButtonImage != null)
            //         {
            //             colorRow.ButtonImage.image =
            //                 colorRow.IsLocked
            //                     ? EditorGUIUtility.IconContent("Locked").image
            //                     : EditorGUIUtility.IconContent("Unlocked").image;
            //         }
            //     }
            //
            //     _currentGlobalLockStatus = !_currentGlobalLockStatus;
            //     lockButtonImage.image = _currentGlobalLockStatus
            //         ? EditorGUIUtility.IconContent("Locked").image
            //         : EditorGUIUtility.IconContent("Unlocked").image;
            // };
            //
            // TODO: Uncomment once all colors options are re-enabled
            // Button randomAllButton = new Button
            // {
            //     text = "R",
            //     style =
            //     {
            //         right = 0
            //     }
            // };
            // allRowContent.Add(randomAllButton);
            //
            // view.Add(allRow);

            _colorSelectionRowView = new VisualElement
            {
                style =
                {
                    width = Length.Percent(100)
                }
            };

            view.Add(_colorSelectionRowView);

            UpdateColorTabContent();
        }

        /// <summary>
        ///     Adds the content to the options tab.
        /// </summary>
        /// <param name="view">The view to add the content to.</param>
        private void AddOptionsTabContent(VisualElement view)
        {
            Label baseAssetLabel = new Label
            {
                style =
                {
                    marginTop = 5,
                    marginLeft = 12,
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                text = "Base Assets",
                tooltip = "These assets are used to construct the character"
            };

            view.Add(baseAssetLabel);

            VisualElement baseModelRow = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    marginLeft = 15,
                    marginRight = 15
                }
            };

            _baseModelField = new ObjectField
            {
                style =
                {
                    flexGrow = 1
                },
                tooltip = "The rigged character model used when constructing a character",
                objectType = typeof(GameObject),
                label = "Model"
            };

            _baseModelField.RegisterCallback<ChangeEvent<Object>>(
                changeEvent =>
                {
                    // TODO: Check the model has a minimum of 1 SkinnedMeshRenderer as a child.
                    if (_sidekickRuntime != null && changeEvent.newValue is GameObject newBaseModel)
                    {
                        _sidekickRuntime.BaseModel = newBaseModel;
                    }

                    // passes the basemodel assigned in the UI into the object state, as it was being ignored.
                    _baseModelField.value = changeEvent.newValue;

                }
            );
            Toggle forceModelToggle = new Toggle
            {
                text = "Force Model",
                value = _forceBaseModel,
                style =
                {
                    marginLeft = 8
                },
                tooltip = "When enabled, characters are always built on the Model assigned above. When disabled, if the selected head part "
                    + "uses a non-standard avatar (e.g. Alien variants), the head's source model is automatically used as the base rig instead."
            };

            forceModelToggle.RegisterValueChangedCallback(
                changeEvent =>
                {
                    _forceBaseModel = changeEvent.newValue;
                    EditorPrefs.SetBool(_FORCE_BASE_MODEL_STATE, _forceBaseModel);
                    if (_sidekickRuntime != null)
                    {
                        _sidekickRuntime.ForceAssignedBaseModel = _forceBaseModel;
                    }
                }
            );

            baseModelRow.Add(_baseModelField);
            baseModelRow.Add(forceModelToggle);
            view.Add(baseModelRow);

            _baseModelField.value = Resources.Load<GameObject>(_BASE_MESH_NAME);

            _materialField = new ObjectField
            {
                tooltip = "The material used when constructing a character",
                objectType = typeof(Material),
                label = "Material",
                style =
                {
                    marginLeft = 15,
                    marginRight = 15
                }
            };

            view.Add(_materialField);

            _materialField.value = Resources.Load<Material>(_BASE_MATERIAL_NAME);

            _animationField = new ObjectField
            {
                tooltip = "The animation controller applied when constructing a character",
                objectType = typeof(AnimatorController),
                label = "Animation Controller",
                value = _currentAnimationController,
                style =
                {
                    marginLeft = 15,
                    marginRight = 15
                }
            };

            view.Add(_animationField);

            _animationField.RegisterValueChangedCallback(
                evt =>
                {
                    _currentAnimationController = (AnimatorController) evt.newValue;
                    _sidekickRuntime.CurrentAnimationController = _currentAnimationController;
                }
            );

            VisualElement updateLibraryLayout = new VisualElement
            {
                style =
                {
                    minHeight = 20,
                    display = DisplayStyle.Flex,
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2,
                    marginTop = 10,
                    marginLeft = 15,
                    marginRight = 2
                }
            };

            Button uploadLibraryButton = new Button()
            {
                text = "Update Part Library",
                tooltip = "Re-scans the project folders to update the parts list"
            };

            uploadLibraryButton.clickable.clicked += delegate
            {
                // Ignore clicks while a rebuild is in flight; CreateGUI already repopulates the tool
                // data and tab content, so a second populate here would race the one it starts.
                if (_loadingContent)
                {
                    return;
                }

                CreateGUI();
            };

            updateLibraryLayout.Add(uploadLibraryButton);
            updateLibraryLayout.Add(_partCountLabel);
            view.Add(updateLibraryLayout);

            Label prefabOptions = new Label
            {
                style =
                {
                    marginTop = 5,
                    marginLeft = 12,
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                text = "Prefab Options",
                tooltip = "Options for how prefabs are created"
            };

            view.Add(prefabOptions);

            _combineMeshesToggle = new Toggle("Combine Character Meshes")
            {
                value = _combineMeshes,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Whether or not to bake all the meshes down to a single mesh in the output model."
            };

            _combineMeshesToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _combineMeshes = evt.newValue;
                    EditorPrefs.SetBool(_COMBINE_MESHES_STATE, _combineMeshes);
                }
            );

            view.Add(_combineMeshesToggle);

            _combineBodyBlendsToggle = new Toggle("Combine Body Blend Shapes")
            {
                value = _combineBodyBlendShapes,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "When enabled the body blend shapes are kept on the output model. "
                    + "When disabled the current body shape is baked into the mesh and the blend shapes are removed."
            };

            _combineBodyBlendsToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _combineBodyBlendShapes = evt.newValue;
                    EditorPrefs.SetBool(_COMBINE_BODY_BLENDS_STATE, _combineBodyBlendShapes);
                }
            );

            view.Add(_combineBodyBlendsToggle);

            _combineFacialBlendsToggle = new Toggle("Combine Facial Blend Shapes")
            {
                value = _combineFacialBlendShapes,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "When enabled the facial blend shapes are kept on the output model. "
                    + "When disabled the current facial blend shape values are baked into the mesh and the blend shapes are removed."
            };

            _combineFacialBlendsToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _combineFacialBlendShapes = evt.newValue;
                    EditorPrefs.SetBool(_COMBINE_FACIAL_BLENDS_STATE, _combineFacialBlendShapes);
                }
            );

            view.Add(_combineFacialBlendsToggle);

            Label toolOptions = new Label
            {
                style =
                {
                    marginTop = 5,
                    marginLeft = 12,
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                text = "Tool Options",
                tooltip = "Options for how the tool behaves"
            };

            view.Add(toolOptions);

            _previewToggle = new Toggle("Auto Build Model")
            {
                value = true,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                }
            };

            //view.Add(_previewToggle);

            Toggle filterColorsToggle = new Toggle("Show all color properties")
            {
                value = _showAllColourProperties,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Display all color properties in the color tab rather than limited to only what the current character is using"
            };

            filterColorsToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _showAllColourProperties = evt.newValue;
                    UpdateVisibleColorSets();
                    UpdateColorTabContent();
                }
            );

            view.Add(filterColorsToggle);

            Toggle autoOpenToggle = new Toggle("Open tool on startup")
            {
                value = _openWindowOnStart,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Opens the Sidekick character tool on Unity startup"
            };

            autoOpenToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _openWindowOnStart = evt.newValue;
                    EditorPrefs.SetBool(_AUTO_OPEN_STATE, _openWindowOnStart);
                }
            );

            view.Add(autoOpenToggle);

            Toggle autoSaveToggle = new Toggle("Remember character")
            {
                value = _useAutoSaveAndLoad,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Auto saves and loads the character when the tool or Unity is opened or closed. "
                    + "The character and tool selections always persist across play mode sessions."
            };

            autoSaveToggle.RegisterValueChangedCallback(
                evt =>
                {
                    _useAutoSaveAndLoad = evt.newValue;
                    EditorPrefs.SetBool(_AUTOSAVE_STATE, _useAutoSaveAndLoad);
                }
            );

            view.Add(autoSaveToggle);

            // TODO: Change to showing presets when parts are missing
            Toggle showMissingParts = new Toggle("Show missing parts warning")
            {
                value = _showMissingPartsPopup,
                style =
                {
                    marginTop = 10,
                    marginLeft = 15
                },
                tooltip = "Shows a popup for missing parts when selecting a preset."
            };

            showMissingParts.RegisterValueChangedCallback(
                evt =>
                {
                    _showMissingPartsPopup = evt.newValue;
                    EditorPrefs.SetBool(_AUTOSAVE_MISSING_PARTS, _showMissingPartsPopup);
                }
            );

            view.Add(showMissingParts);

            VisualElement syncDatabaseLayout = new VisualElement
            {
                style =
                {
                    minHeight = 20,
                    display = DisplayStyle.Flex,
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2,
                    marginTop = 10,
                    marginLeft = 15,
                    marginRight = 2
                }
            };

            Button syncDatabaseButton = new Button(DatabaseBuildSync.SyncRuntimeDatabase)
            {
                text = "Sync Runtime Database",
                tooltip = "Copies the Sidekick database into the plugin's Resources folder as a .bytes asset so that player builds "
                    + "can read the part, preset and color data at runtime. This sync also runs automatically before every build; "
                    + "use this button to refresh the copy manually after the database has been updated."
            };

            syncDatabaseLayout.Add(syncDatabaseButton);
            view.Add(syncDatabaseLayout);

            VisualElement row = new VisualElement
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    justifyContent = new StyleEnum<Justify>(Justify.SpaceAround),
                    marginTop = 30
                }
            };

            Button documentationButton = new Button
            {
                text = "Documentation",
                style =
                {
                    width = Length.Percent(30)
                },
                tooltip = "Open documentation for Sidekick characters"
            };

            documentationButton.clickable.clicked += delegate
            {
                Application.OpenURL("file:Assets/Synty/SidekickCharacters/Documentation/SidekickCharacters_UserGuide.pdf");
            };

            Button storeButton = new Button
            {
                text = "Synty Store",
                style =
                {
                    width = Length.Percent(30)
                },
                tooltip = "www.syntystore.com"
            };

            storeButton.clickable.clicked += delegate
            {
                Application.OpenURL("https://syntystore.com");
            };

            Button tutorialButton = new Button
            {
                text = "Tutorials",
                style =
                {
                    width = Length.Percent(30)
                },
                tooltip = "Sidekick Characters - Quick start guide"
            };

            tutorialButton.clickable.clicked += delegate
            {
                // TODO: Change to direct link to tutorial playlist, when available
                Application.OpenURL("https://www.youtube.com/@syntystudios");
            };

            row.Add(documentationButton);
            row.Add(storeButton);
            row.Add(tutorialButton);
            view.Add(row);
        }

        /// <summary>
        ///     Reset the color rows for this color set back to the colors stored on the saved textures.
        /// </summary>
        private void ResetColorSet()
        {
            PopulateColorRowsFromTextures();
        }

        /// <summary>
        ///     Shows the name field and creation button for creating a new color set.
        /// </summary>
        private void ShowCreateNewColorSet()
        {
            _newSetNameContainer.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        ///     Creates a new color set with the given name.
        /// </summary>
        /// <param name="setName">The name for the color set.</param>
        private void CreateNewColorSet(string setName)
        {
            ResetCurrentColorSet(setName);
            SaveColorSet();
        }

        /// <summary>
        ///     Sets the current color set to a new, in-memory set of rows independent of the database
        /// </summary>
        /// <param name="setName">Name of the new color set</param>
        private void ResetCurrentColorSet(string setName = "Custom")
        {
            _currentColorSet.ID = -1;
            _currentColorSet.Species = _currentSpecies;
            _currentColorSet.Name = setName;

            foreach (SidekickColorRow row in _allColorRows)
            {
                row.ID = -1;
                row.ColorSet = _currentColorSet;
            }
        }

        /// <summary>
        ///     Deletes a color set from the database. The color set will still be available in the app until the app is closed.
        /// </summary>
        private void DeleteColorSet()
        {
            List<SidekickColorRow> rowsToDelete = SidekickColorRow.GetAllBySet(_dbManager, _currentColorSet);
            foreach (SidekickColorRow row in rowsToDelete)
            {
                row.Delete(_dbManager);
            }

            _currentColorSet.Delete(_dbManager);
            UpdateVisibleColorSets();
            ResetCurrentColorSet();

        }

        /// <summary>
        ///     Saves the current color row to the database. If it is a new color row, it is inserted into the DB; otherwise it is updated in the DB.
        /// </summary>
        private void SaveColorSet()
        {
            string path = Path.Combine(_BASE_COLOR_SET_PATH, _currentSpecies.Name);
            path = Path.Combine(path, _currentColorSet.Name.Replace(" ", "_"));

            SaveTexturesToDisk(path);

            _currentColorSet.Save(_dbManager);
            foreach (SidekickColorRow row in _allColorRows)
            {
                row.Save(_dbManager);
            }

            UpdateVisibleColorSets(false);

            // TODO : refresh the project inspector window so the new textures show up
        }

        /// <summary>
        ///     Saves texture files to disk at the given path.
        /// </summary>
        /// <param name="path">The path to save the textures to.</param>
        private void SaveTexturesToDisk(string path, string additionalNaming = "")
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // if no parts are selected, don't try and save a non-existent material, instead create one
            if (_currentMaterial == null)
            {
                _currentMaterial = (Material) _materialField.value;
            }

            string filename = _TEXTURE_PREFIX;

            if (!string.IsNullOrEmpty(additionalNaming))
            {
                filename += additionalNaming;
            }

            string filePath = Path.Combine(path, filename + _TEXTURE_COLOR_NAME);
            Texture2D texture = (Texture2D) _currentMaterial.GetTexture(_COLOR_MAP);
            File.WriteAllBytes(filePath, texture.EncodeToPNG());
            _currentColorSet.SourceColorPath = filePath;
            // TODO: Hidden due to early access, enable when feature complete
            // filePath = Path.Combine(path, filename + _TEXTURE_METALLIC_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_METALLIC_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceMetallicPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_SMOOTHNESS_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_SMOOTHNESS_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceSmoothnessPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_REFLECTION_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_REFLECTION_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceReflectionPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_EMISSION_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_EMISSION_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceEmissionPath = filePath;
            // filePath = Path.Combine(path, filename + _TEXTURE_OPACITY_NAME);
            // texture = (Texture2D) _currentMaterial.GetTexture(_OPACITY_MAP);
            // File.WriteAllBytes(filePath, texture.EncodeToPNG());
            // _currentColorSet.SourceOpacityPath = filePath;
        }

        /// <summary>
        ///     Updates the color sets that are selectable in the dropdown on the colors tab
        /// </summary>
        /// <param name="setDropdownToCustom">Whether to set the color sets dropdown value to 'Custom'</param>
        private void UpdateVisibleColorSets(bool setDropdownToCustom = true)
        {
            List<SidekickColorSet> sets = SidekickColorSet.GetAllBySpecies(_dbManager, _currentSpecies);
            if (sets.Count == 0)
            {
                sets.Add(SidekickColorSet.GetDefault(_dbManager));
            }

            List<string> setNames = sets.Select(set => set.Name).ToList();
            _colorSetsDropdown.choices = setNames;
            _visibleColorSets = sets;

            if (setDropdownToCustom)
            {
                _colorSetsDropdown.value = "Custom";
            }
        }

        /// <summary>
        ///     Updates all the color rows currently visible in the UI.
        /// </summary>
        private void UpdateAllVisibleColors()
        {
            foreach (SidekickColorRow row in _visibleColorRows)
            {
                UpdateAllColors(row);
            }
        }

        /// <summary>
        ///     Updates every color row (visible or not) in the color map textures.
        /// </summary>
        private void UpdateAllColors()
        {
            foreach (SidekickColorRow row in _allColorRows)
            {
                UpdateAllColors(row);
            }
        }

        /// <summary>
        ///     Updates all the color types for a given color row.
        /// </summary>
        /// <param name="colorRow">The color row to update.</param>
        private void UpdateAllColors(SidekickColorRow colorRow)
        {
            foreach (ColorType colorType in Enum.GetValues(typeof(ColorType)))
            {
                _sidekickRuntime.UpdateColor(colorType, colorRow);
            }
        }

        /// <summary>
        ///     Populates the part color rows based on the filter being used.
        /// </summary>
        private void PopulatePartColorRows()
        {
            List<SidekickColorProperty> propertiesToShow = new List<SidekickColorProperty>();

            switch (_currentPartType)
            {
                case ColorPartType.Species:
                    List<SidekickColorProperty> speciesProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Species);
                    foreach (SidekickColorProperty property in speciesProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Outfit:
                    List<SidekickColorProperty> outfitProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Outfits);
                    foreach (SidekickColorProperty property in outfitProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Attachments:
                    List<SidekickColorProperty> attachmentProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Attachments);
                    foreach (SidekickColorProperty property in attachmentProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Materials:
                    List<SidekickColorProperty> materialProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Materials);
                    foreach (SidekickColorProperty property in materialProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Elements:
                    List<SidekickColorProperty> elementProperties = SidekickColorProperty.GetAllByGroup(_dbManager, ColorGroup.Elements);
                    foreach (SidekickColorProperty property in elementProperties)
                    {
                        Vector2 uv = new Vector2(property.U, property.V);
                        if ((_currentUVList.Contains(uv) || _showAllColourProperties == true) && !propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.CharacterHead:
                    List<SidekickColorProperty> headProperties = new List<SidekickColorProperty>();
                    foreach (ColorPartType type in ColorPartType.CharacterHead.GetPartTypes())
                    {
                        headProperties.AddRange(SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[type]));
                    }

                    foreach (SidekickColorProperty property in headProperties)
                    {
                        if (!propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.CharacterUpperBody:
                    List<SidekickColorProperty> upperProperties = new List<SidekickColorProperty>();
                    foreach (ColorPartType type in ColorPartType.CharacterUpperBody.GetPartTypes())
                    {
                        upperProperties.AddRange(SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[type]));
                    }

                    foreach (SidekickColorProperty property in upperProperties)
                    {
                        if (!propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.CharacterLowerBody:
                    List<SidekickColorProperty> lowerProperties = new List<SidekickColorProperty>();
                    foreach (ColorPartType type in ColorPartType.CharacterLowerBody.GetPartTypes())
                    {
                        lowerProperties.AddRange(SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[type]));
                    }

                    foreach (SidekickColorProperty property in lowerProperties)
                    {
                        if (!propertiesToShow.Contains(property))
                        {
                            propertiesToShow.Add(property);
                        }
                    }

                    break;
                case ColorPartType.Head:
                case ColorPartType.Hair:
                case ColorPartType.EyebrowLeft:
                case ColorPartType.EyebrowRight:
                case ColorPartType.EyeLeft:
                case ColorPartType.EyeRight:
                case ColorPartType.EarLeft:
                case ColorPartType.EarRight:
                case ColorPartType.FacialHair:
                case ColorPartType.Torso:
                case ColorPartType.ArmUpperLeft:
                case ColorPartType.ArmUpperRight:
                case ColorPartType.ArmLowerLeft:
                case ColorPartType.ArmLowerRight:
                case ColorPartType.HandLeft:
                case ColorPartType.HandRight:
                case ColorPartType.Hips:
                case ColorPartType.LegLeft:
                case ColorPartType.LegRight:
                case ColorPartType.FootLeft:
                case ColorPartType.FootRight:
                case ColorPartType.AttachmentHead:
                case ColorPartType.AttachmentFace:
                case ColorPartType.AttachmentBack:
                case ColorPartType.AttachmentHipsFront:
                case ColorPartType.AttachmentHipsBack:
                case ColorPartType.AttachmentHipsLeft:
                case ColorPartType.AttachmentHipsRight:
                case ColorPartType.AttachmentShoulderLeft:
                case ColorPartType.AttachmentShoulderRight:
                case ColorPartType.AttachmentElbowLeft:
                case ColorPartType.AttachmentElbowRight:
                case ColorPartType.AttachmentKneeLeft:
                case ColorPartType.AttachmentKneeRight:
                case ColorPartType.Nose:
                case ColorPartType.Teeth:
                case ColorPartType.Tongue:
                case ColorPartType.Wrap:/*
                case ColorPartType.AttachmentHandLeft:
                case ColorPartType.AttachmentHandRight: */
                    propertiesToShow = SidekickColorProperty.GetByUVs(_dbManager, _currentUVDictionary[_currentPartType]);
                    break;
                case ColorPartType.AllParts:
                default:
                    propertiesToShow = _showAllColourProperties || _currentUVList == null
                        ? SidekickColorProperty.GetAll(_dbManager)
                        : SidekickColorProperty.GetByUVs(_dbManager, _currentUVList);
                    break;
            }

            _visibleColorRows.Clear();

            // when filtering the view to a specific UV dictionary, we need to reset the property order
            propertiesToShow.Sort((a, b) => a.ID.CompareTo(b.ID));

            foreach (SidekickColorProperty property in propertiesToShow)
            {
                foreach (SidekickColorRow row in _allColorRows.Where(row => row.ColorProperty.ID == property.ID))
                {
                    _visibleColorRows.Add(row);
                }
            }
        }

        /// <summary>
        ///     Refreshes the visible color rows in the UI.
        /// </summary>
        private void RefreshVisibleColorRows()
        {
            _colorSelectionRowView.Clear();

            foreach (ColorGroup group in Enum.GetValues(typeof(ColorGroup)))
            {
                List<SidekickColorProperty> properties = _visibleColorRows
                    .Select(row => row.ColorProperty)
                    .Where(prop => prop.Group == group)
                    .ToList();

                string tooltipText = "";

                switch (group)
                {
                    case ColorGroup.Species:
                        tooltipText = "Species colors make up the character as if it has no outfit on. (for example - skin, teeth, tongue, fingernails etc)";
                        break;
                    case ColorGroup.Outfits:
                        tooltipText = "Outfit colors make up the clothing on the character. (for example - Torso outfit, arm outfit, hand outfit etc)";
                        break;
                    case ColorGroup.Attachments:
                        tooltipText = "Attachment colors make up the additional parts attached to a character. (for example - a backpack, shoulder pads, elbow pads, knee pads etc)";
                        break;
                    case ColorGroup.Materials:
                        tooltipText = "material colors make up a collection of shared standard materials (for example - wood, metal, leather, paper, bone etc)";
                        break;
                }

                if (properties.Count > 0)
                {
                    Label groupLabel = new Label(group.ToString())
                    {
                        style =
                        {
                            unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                            marginBottom = 4,
                            marginTop = 6
                        },
                        tooltip = tooltipText
                    };
                    _colorSelectionRowView.Add(groupLabel);

                    VisualElement headerContainer = new VisualElement()
                    {
                        style =
                        {
                            flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                            fontSize = 10,
                            unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                        }
                    };

                    Label colorHeader = new Label("Color")
                    {
                        style =
                        {
                            width = 103,
                            marginLeft = 155
                        }
                    };
                    Label metallicHeader = new Label("Metallic")
                    {
                        style =
                        {
                            width = 66
                        }
                    };
                    Label smoothnessHeader = new Label("Smoothness")
                    {
                        style =
                        {
                            width = 66
                        }
                    };
                    Label reflectionHeader = new Label("Reflection")
                    {
                        style =
                        {
                            width = 66
                        }
                    };
                    Label emissionHeader = new Label("Emission")
                    {
                        style =
                        {
                            width = 66
                        }
                    };
                    Label opacityHeader = new Label("Opacity")
                    {
                        style =
                        {
                            width = 66
                        }
                    };

                    headerContainer.Add(colorHeader);
                    // headerContainer.Add(metallicHeader);
                    // headerContainer.Add(smoothnessHeader);
                    // headerContainer.Add(reflectionHeader);
                    // headerContainer.Add(emissionHeader);
                    // headerContainer.Add(opacityHeader);

                    _colorSelectionRowView.Add(headerContainer);

                    foreach (SidekickColorProperty property in properties)
                    {
                        foreach (SidekickColorRow row in _visibleColorRows.Where(row => row.ColorProperty.ID == property.ID))
                        {
                            CreateColorRow(_colorSelectionRowView, row);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Populates the color rows from texture files on the disk.
        /// </summary>
        /// <param name="overrideColorMap">
        ///     Optional color map to sample instead of the current color set's source texture; used when detecting the
        ///     colors of a character already in the scene. Ignored when unusable (unreadable non-asset, or too small).
        /// </param>
        private void PopulateColorRowsFromTextures(Texture2D overrideColorMap = null)
        {
            TextureImporter textureImporter = null;

            Texture2D mainColor = null;

            if (overrideColorMap != null)
            {
                if (!overrideColorMap.isReadable)
                {
                    string overridePath = AssetDatabase.GetAssetPath(overrideColorMap);
                    if (!string.IsNullOrEmpty(overridePath) && AssetImporter.GetAtPath(overridePath) is TextureImporter overrideImporter)
                    {
                        overrideImporter.isReadable = true;
                        overrideImporter.SaveAndReimport();
                    }
                }

                // GetPixel below assumes the 32x32 (2x2 pixels per color cell) layout.
                if (overrideColorMap.isReadable && overrideColorMap.width >= 32 && overrideColorMap.height >= 32)
                {
                    overrideColorMap.filterMode = FilterMode.Point;
                    mainColor = overrideColorMap;
                }
            }

            if (mainColor == null)
            {
                mainColor = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceColorPath);
                if (mainColor != null)
                {
                    mainColor.filterMode = FilterMode.Point;
                    if (!mainColor.isReadable)
                    {
                        textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceColorPath);
                        textureImporter.isReadable = true;
                        textureImporter.SaveAndReimport();
                    }
                }
                else
                {
                    // The Sidekick shader has no main texture, so mainTexture is always null; the color map
                    // property is the texture the tool paints and holds the current colors of this session.
                    Material defaultMaterial = (Material) _materialField.value;
                    mainColor = defaultMaterial != null ? defaultMaterial.GetTexture(_COLOR_MAP) as Texture2D : null;
                }
            }

            Texture2D metallic = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceMetallicPath);
            if (metallic != null)
            {
                metallic.filterMode = FilterMode.Point;
                if (!metallic.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceMetallicPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D smoothness = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceSmoothnessPath);
            if (smoothness != null)
            {
                smoothness.filterMode = FilterMode.Point;
                if (!smoothness.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceSmoothnessPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D reflection = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceReflectionPath);
            if (reflection != null)
            {
                reflection.filterMode = FilterMode.Point;
                if (!reflection.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceReflectionPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D emission = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceEmissionPath);
            if (emission != null)
            {
                emission.filterMode = FilterMode.Point;
                if (!emission.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceEmissionPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            Texture2D opacity = AssetDatabase.LoadAssetAtPath<Texture2D>(_currentColorSet.SourceOpacityPath);
            if (opacity != null)
            {
                opacity.filterMode = FilterMode.Point;
                if (!opacity.isReadable)
                {
                    textureImporter = (TextureImporter) AssetImporter.GetAtPath(_currentColorSet.SourceOpacityPath);
                    textureImporter.isReadable = true;
                    textureImporter.SaveAndReimport();
                }
            }

            List<SidekickColorRow> newColorRows = new List<SidekickColorRow>();

            List<SidekickColorRow> currentSetColors = SidekickColorRow.GetAllBySet(_dbManager, _currentColorSet);

            // TODO : if textures don't exist BUT color rows exist in DB, ask user if they want to re-save the textures from the DB values, loop back and reimport
            // TODO : if textures don't exist AND color rows don't exist in DB, delete the colorset entry in DB/dropdown, advance to next on list and reload

            foreach (SidekickColorProperty property in SidekickColorProperty.GetAll(_dbManager))
            {
                SidekickColorRow existingRow = currentSetColors.FirstOrDefault(row => row.ColorProperty.ID == property.ID);
                SidekickColorRow newRow = new SidekickColorRow
                {
                    ID = existingRow?.ID ?? -1,
                    ColorSet = _currentColorSet,
                    ColorProperty = property,
                    // TODO remove null checks when we know we have textures
                    NiceColor = mainColor?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceColor ?? Color.red,
                    // NiceMetallic = metallic?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceMetallic ?? Color.red,
                    // NiceSmoothness = smoothness?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceSmoothness ?? Color.red,
                    // NiceReflection = reflection?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceReflection ?? Color.red,
                    // NiceEmission = emission?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceEmission ?? Color.red,
                    // NiceOpacity = opacity?.GetPixel(property.U * 2, property.V * 2) ?? existingRow?.NiceOpacity ?? Color.red
                };

                newRow.Save(_dbManager);
                newColorRows.Add(newRow);
            }

            _allColorRows = newColorRows;
            PopulatePartColorRows();
            RefreshVisibleColorRows();
        }

        /// <summary>
        ///     Adds a color row to the given view.
        /// </summary>
        /// <param name="view">The view to add the color row to.</param>
        /// <param name="colorRow">The color row to populate this UI element with.</param>
        private void CreateColorRow(VisualElement view, SidekickColorRow colorRow)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("colorSelectionRow");

            Label rowLabel = new Label(colorRow.ColorProperty.Name);
            rowLabel.AddToClassList("colorSelectionRowLabel");
            row.Add(rowLabel);

            VisualElement rowContent = new VisualElement();
            rowContent.AddToClassList("colorSelectionRowContent");

            row.Add(rowContent);

            // TODO: uncomment when locking is required.
            // Button btnLock = new Button();
            //
            // Image lockImage = new Image
            // {
            //     image = colorRow.IsLocked ? EditorGUIUtility.IconContent("Locked").image : EditorGUIUtility.IconContent("Unlocked").image,
            //     scaleMode = ScaleMode.ScaleToFit
            // };
            //
            // btnLock.Add(lockImage);
            // colorRow.ButtonImage = lockImage;
            // rowContent.Add(btnLock);
            // btnLock.clickable.clicked += () =>
            // {
            //     colorRow.IsLocked = !colorRow.IsLocked;
            //     lockImage.image = colorRow.IsLocked
            //         ? EditorGUIUtility.IconContent("Locked").image
            //         : EditorGUIUtility.IconContent("Unlocked").image;
            // };

            ColorField colorField = new ColorField
            {
                value = colorRow.NiceColor,
                tooltip = colorRow.ColorProperty.Name + " Color",
                style =
                {
                    // TODO: shrink to 50 once all colors options are re-enabled
                    width = 100
                }
            };
            rowContent.Add(colorField);
            colorField.RegisterValueChangedCallback(
                evt =>
                {
                    // An undo can rebuild the rows while the color picker is still open; ignore events from a
                    // field whose row is no longer part of the tool state.
                    if (_isApplyingUndoState || !_allColorRows.Contains(colorRow))
                    {
                        return;
                    }

                    RecordUndo("Sidekick Color Change");
                    colorRow.NiceColor = evt.newValue;
                    _sidekickRuntime.UpdateColor(ColorType.MainColor, colorRow);
                    CommitUndoState();
                }
            );

            // TODO: Uncomment once all colors options are re-enabled
            // ColorField metallicField = new ColorField
            // {
            //     value = colorRow.NiceMetallic,
            //     tooltip = colorRow.ColorProperty.Name + " Metallic",
            //     style =
            //     {
            //         width = 60
            //     }
            // };
            // rowContent.Add(metallicField);
            // metallicField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceMetallic = evt.newValue;
            //         _sidekickRuntime.UpdateColor(ColorType.Metallic, colorRow);
            //     }
            // );
            //
            // ColorField smoothnessField = new ColorField
            // {
            //     value = colorRow.NiceSmoothness,
            //     tooltip = colorRow.ColorProperty.Name + " Smoothness",
            //     style =
            //     {
            //         width = 60
            //     }
            // };
            // rowContent.Add(smoothnessField);
            // smoothnessField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceSmoothness = evt.newValue;
            //         _sidekickRuntime.UpdateColor(ColorType.Smoothness, colorRow);
            //     }
            // );
            //
            // ColorField reflectionField = new ColorField
            // {
            //     value = colorRow.NiceReflection,
            //     tooltip = colorRow.ColorProperty.Name + " Reflection",
            //     style =
            //     {
            //         width = 60
            //     }
            // };
            // rowContent.Add(reflectionField);
            // reflectionField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceReflection = evt.newValue;
            //         _sidekickRuntime.UpdateColor(ColorType.Reflection, colorRow);
            //     }
            // );
            //
            // ColorField emissionField = new ColorField
            // {
            //     value = colorRow.NiceEmission,
            //     tooltip = colorRow.ColorProperty.Name + " Emission",
            //     style =
            //     {
            //         width = 60
            //     }
            // };
            // rowContent.Add(emissionField);
            // emissionField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceEmission = evt.newValue;
            //         _sidekickRuntime.UpdateColor(ColorType.Emission, colorRow);
            //     }
            // );
            //
            // ColorField opacityField = new ColorField
            // {
            //     value = colorRow.NiceOpacity,
            //     tooltip = colorRow.ColorProperty.Name + " Opacity",
            //     style =
            //     {
            //         width = 60
            //     }
            // };
            // rowContent.Add(opacityField);
            // opacityField.RegisterValueChangedCallback(
            //     evt =>
            //     {
            //         colorRow.NiceOpacity = evt.newValue;
            //         _sidekickRuntime.UpdateColor(ColorType.Opacity, colorRow);
            //     }
            // );

            // Button randomButton = new Button
            // {
            //     text = "R",
            //     style =
            //     {
            //         right = 0
            //     }
            // };
            // rowContent.Add(randomButton);

            view.Add(row);
        }

        /// <summary>
        ///     Switches the currently visible tab to the given tab.
        /// </summary>
        /// <param name="newTab">The tab to switch to.</param>
        private void SwitchToTab(TabView newTab)
        {
            if (_currentTab == newTab)
            {
                return;
            }

            _currentTab = newTab;

            _bodyPresetTab.value = _currentTab == TabView.Preset;
            _bodyPartsTab.value = _currentTab == TabView.Parts;
            _bodyShapeTab.value = _currentTab == TabView.Body;
            _colorSelectionTab.value = _currentTab == TabView.Colors;
            // _decalSelectionTab.value = _currentTab == TabView.Decals;
            _optionTab.value = _currentTab == TabView.Options;

            _presetView.style.display = _bodyPresetTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _partView.style.display = _bodyPartsTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _bodyShapeView.style.display = _bodyShapeTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _colorSelectionView.style.display = _colorSelectionTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            //_decalSelectionView.style.display = _decalSelectionTab.value ? DisplayStyle.Flex : DisplayStyle.None;
            _optionSelectionView.style.display = _optionTab.value ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        ///     Populate the preset tab content.
        /// </summary>
        private void PopulatePresetUI()
        {
            _presetView.Clear();
            _presetRowControls.Clear();

            Dictionary<string, PopupField<string>> dropdowns = new Dictionary<string, PopupField<string>>();

            Foldout speciesFoldout = new Foldout
            {
                text = "Select - Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
            };

            List<string> speciesNames = _allSpecies.Select(species => species.Name).ToList();

            _speciesPresetField = new DropdownField
            {
                label = "Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal),
                    marginRight = 20
                },
                tooltip = "Select the species of your character"
            };
            _speciesPresetField.choices = speciesNames;
            ScrollableDropdown.Apply(_speciesPresetField);
            _speciesPresetField.RegisterValueChangedCallback(
                evt =>
                {
                    _speciesField.value = _speciesPresetField.value;
                    ProcessSpeciesChange(evt.newValue);
                }
            );

            _speciesPresetField.index = _currentSpecies != null && speciesNames.Count > 0 ? _speciesPresetField.choices.IndexOf(_currentSpecies.Name) : 0;
            speciesFoldout.Add(_speciesPresetField);
            _presetView.Add(speciesFoldout);

            List<SidekickPresetFilter> allFilters = SidekickPresetFilter.GetAll(_dbManager);

            allFilters.Sort(
                (filterA, filterB) => String.CompareOrdinal(filterA.Term, filterB.Term)
            );

            if (allFilters.Count > 0)
            {
                Foldout filterFoldout = new Foldout()
                {
                    text = "Select - Preset Part Filter",
                    style =
                    {
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                    }
                };

                VisualElement filterContent = new VisualElement
                {
                    style =
                    {
                        flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                        flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
                    }
                };

                Color borderColor = new Color(0.17f, 0.17f, 0.17f);
                Color backgroundColor = new Color(0.35f, 0.35f, 0.35f);
                List<Toggle> allFilterToggles = new List<Toggle>();

                foreach (SidekickPresetFilter filter in allFilters)
                {
                    Toggle outfitToggle = new Toggle(filter.Term)
                    {
                        value = !_partPresetFilterToggleMap.TryGetValue(filter, out bool toggleValue) || toggleValue,
                        style =
                        {
                            width = 160,
                            borderBottomWidth = 1,
                            borderBottomColor = borderColor,
                            paddingBottom = 2,
                            borderLeftWidth = 1,
                            borderLeftColor = borderColor,
                            paddingLeft = 2,
                            borderRightWidth = 1,
                            borderRightColor = borderColor,
                            paddingRight = 2,
                            borderTopWidth = 1,
                            borderTopColor = borderColor,
                            paddingTop = 2,
                            borderBottomLeftRadius = 3,
                            borderBottomRightRadius = 3,
                            borderTopLeftRadius = 3,
                            borderTopRightRadius = 3,
                            backgroundColor = backgroundColor,
                            textOverflow = new StyleEnum<TextOverflow>(TextOverflow.Ellipsis)
                        }
                    };

                    allFilterToggles.Add(outfitToggle);

                    if (outfitToggle.value)
                    {
                        _partPresetFilterToggleMap[filter] = outfitToggle.value;
                    }

                    outfitToggle.RegisterValueChangedCallback(
                        evt =>
                        {
                            _partPresetFilterToggleMap[filter] = evt.newValue;
                            PopulatePresetPartDropdowns(dropdowns);
                        }
                    );

                    filterContent.Add(outfitToggle);
                }

                VisualElement buttonRow = new VisualElement()
                {
                    style =
                    {
                        flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                    }
                };

                Button selectAll = new Button(
                    delegate
                    {
                        foreach (Toggle toggle in allFilterToggles)
                        {
                            toggle.value = true;
                        }
                    }
                )
                {
                    text = "Select All"
                };

                Button selectNone = new Button(
                    delegate
                    {
                        foreach (Toggle toggle in allFilterToggles)
                        {
                            toggle.value = false;
                        }
                    }
                )
                {
                    text = "Select None"
                };

                buttonRow.Add(selectAll);
                buttonRow.Add(selectNone);

                filterFoldout.Add(buttonRow);
                filterFoldout.Add(filterContent);

                _presetView.Add(filterFoldout);
            }

            _availablePresets = SidekickPartPreset.GetAll(_dbManager);

            Foldout generateFoldout = new Foldout
            {
                text = "Randomize Character",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                //tooltip = "Create a character based on the selected species"
            };

            Button generateButton = new Button()
            {
                style =
                {
                    minHeight = 50,
                    marginRight = 18,
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    alignContent = new StyleEnum<Align>(Align.Center),
                    alignItems = new StyleEnum<Align>(Align.Center),
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                    justifyContent = new StyleEnum<Justify>(Justify.Center)
                },
                tooltip = "Generate a character at the push of a button"
            };

            Texture2D randomImage = Resources.Load<Texture2D>("UI/T_Random");
            generateButton.Add(
                new Image
                {
                    image = randomImage,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingTop = new StyleLength(1),
                        paddingBottom = new StyleLength(1),
                        paddingRight = 5,
                        alignSelf = new StyleEnum<Align>(Align.Center)
                    }
                }
            );
            generateButton.Add(
                new Label
                {
                    text = "Randomize Character",
                    style =
                    {
                        alignSelf = new StyleEnum<Align>(Align.Center)
                    }
                }
            );

            generateFoldout.Add(generateButton);
            _presetView.Add(generateFoldout);

            Foldout presetsFoldout = new Foldout()
            {
                text = "Presets",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                //tooltip = "Select from a number of collections of parts, body types and colors"
            };

            Label partTitle = new Label("Parts")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                tooltip = "Collections of character parts ie. Head attachment, Torso, Nose that make up the character"
            };

            string tooltipText = "";

            presetsFoldout.Add(partTitle);
            _presetView.Add(presetsFoldout);

            _presetPartContainer = new VisualElement();
            presetsFoldout.Add(_presetPartContainer);

            PopulatePresetPartDropdowns(dropdowns);

            Label bodyTitle = new Label("Body")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                tooltip = "Preset bodies in a number of types, sizes and musculature"
            };

            presetsFoldout.Add(bodyTitle);
            _currentBodyPresetDictionary = new Dictionary<string, SidekickBodyShapePreset>();
            List<SidekickBodyShapePreset> bodyShapes = SidekickBodyShapePreset.GetAll(_dbManager);
            List<string> bodyShapeNames = bodyShapes.Select(b => b.Name).ToList();
            for (int i = 0; i < bodyShapeNames.Count; i++)
            {
                _currentBodyPresetDictionary.Add(bodyShapeNames[i], bodyShapes[i]);
            }

            string bodyTypeLabel = "Body Type";

            string bodyShapeDefaultValue = "Androgynous Medium";

            if (bodyShapeNames.Count > 0)
            {
                bodyShapeDefaultValue = _presetDefaultValues.TryGetValue(bodyTypeLabel, out string bodyShapeValue)
                    ? bodyShapeValue
                    : bodyShapeDefaultValue;
            }

            bodyShapeNames.Sort();

            tooltipText = "Select a body preset for you character - a body type preset is made up of combinations of body type, size and musculature.";

            dropdowns[bodyTypeLabel] = CreatePresetRow(presetsFoldout, bodyTypeLabel, tooltipText, bodyShapeNames, false, bodyShapeDefaultValue, PresetDropdownType.Body);

            Label colorTitle = new Label("Colors")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                },
                tooltip = "Collections of character colors ie. Skin color, teeth color, eye color, hair color that make up the characters colors"
            };

            SidekickSpecies unrestrictedSpecies = SidekickSpecies.GetByName(_dbManager, "Unrestricted");

            presetsFoldout.Add(colorTitle);
            _currentColorSpeciesPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorOutfitsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorAttachmentsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorMaterialsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            _currentColorElementsPresetDictionary = new Dictionary<string, SidekickColorPreset>();
            foreach (ColorGroup colorGroup in Enum.GetValues(typeof(ColorGroup)))
            {
                // TODO: remove when Element colors are re-added
                if (colorGroup == ColorGroup.Elements)
                {
                    continue;
                }

                List<SidekickColorPreset> colorPresets = colorGroup is ColorGroup.Species && _currentSpecies.ID != unrestrictedSpecies.ID
                    ? SidekickColorPreset.GetAllByColorGroupAndSpecies(_dbManager, colorGroup, _currentSpecies)
                    : SidekickColorPreset.GetAllByColorGroup(_dbManager, colorGroup);

                List<string> colorPresetNames = colorPresets.Select(cp => cp.Name).ToList();
                for (int i = 0; i < colorPresetNames.Count; i++)
                {
                    switch (colorGroup)
                    {
                        case ColorGroup.Species:
                            _currentColorSpeciesPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select a species color preset for your character - a species color preset is made up of the colors that would make up the character if it had no outfit on. (for example - skin, teeth, tongue, fingernails etc)";
                            break;
                        case ColorGroup.Outfits:
                            _currentColorOutfitsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select an outfit color preset for your character - an outfit color preset is made up of the colors that make up the clothing on the character. (for example - torso outfit, arm outfit, hand outfit etc)";
                            break;
                        case ColorGroup.Attachments:
                            _currentColorAttachmentsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select an attachments color preset for your character - an attachments color preset is made up of the colors used on additional parts on the character. (for example - shoulder attachments, back attachments, hip attachments etc)";
                            break;
                        case ColorGroup.Materials:
                            _currentColorMaterialsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            tooltipText = "Select a materials color preset for your character - a materials color preset is made up of the colors that make up general materials of the outfit and attachments. (for example - metal, wood, leather, plastic, bone etc)";
                            break;
                        case ColorGroup.Elements:
                            _currentColorElementsPresetDictionary.Add(colorPresetNames[i], colorPresets[i]);
                            break;
                    }
                }

                string defaultValue = "None";

                if (colorPresetNames.Count > 0)
                {
                    defaultValue = _presetDefaultValues.TryGetValue(colorGroup.ToString(), out string value)
                        ? value
                        : defaultValue;
                }

                colorPresetNames.Sort();

                if (_processingSpeciesChange && colorGroup == ColorGroup.Species && !_loadingCharacter)
                {
                    defaultValue = colorPresetNames.Count > 0 ? colorPresetNames[0] : "None";
                }

                dropdowns[colorGroup.ToString()] = CreatePresetRow(presetsFoldout, colorGroup.ToString(), tooltipText, colorPresetNames, true, defaultValue, PresetDropdownType.Color);
            }

            /* TODO: When decals are added (issue 1108), uncomment and update this section
            Label textureTitle = new Label("Textures")
            {
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            presetsFoldout.Add(textureTitle);
            CreatePresetRow(presetsFoldout, "Skin", new List<string>(), true, "None", PresetDropdownType.Texture);
            CreatePresetRow(presetsFoldout, "Outfit", new List<string>(), true, "None", PresetDropdownType.Texture);
            */

            generateButton.clickable.clicked += delegate
            {
                // Ignore clicks while a randomize/preset batch is still being applied: _applyingPreset stays
                // true until the debounced rebuild completes, so this prevents overlapping randomize batches
                // (which share _applyingPreset / _reapplyColorsOnRegen state) and collapses any stray
                // double-dispatch of the click into a single randomize.
                if (_applyingPreset)
                {
                    return;
                }

                RecordUndo("Randomize Character");
                _applyingPreset = true;
                foreach (PopupField<string> dropdown in dropdowns.Values)
                {
                    List<string> values = dropdown.choices;
                    values.Remove("None");
                    string newValue = "None";
                    if (values.Count > 0)
                    {
                        newValue = values[Random.Range(0, values.Count - 1)];
                    }
                    dropdown.value = newValue;
                }

                SchedulePreviewRegeneration();
            };
        }

        /// <summary>
        ///     Populates the preset part dropdowns.
        /// </summary>
        /// <param name="dropdowns">The list of all preset dropdowns to put the preset part dropdowns into once populated
        /// </param>
        private void PopulatePresetPartDropdowns(Dictionary<string, PopupField<string>> dropdowns)
        {
            _presetPartContainer.Clear();
            string tooltipText = "";
            _currentHeadPresetDictionary = new Dictionary<string, SidekickPartPreset>();
            _currentUpperBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
            _currentLowerBodyPresetDictionary = new Dictionary<string, SidekickPartPreset>();
            HashSet<SidekickPartPreset> mappedPresets = new HashSet<SidekickPartPreset>();

            if (_partPresetFilterToggleMap.Count > 0)
            {
                foreach (KeyValuePair<SidekickPresetFilter, bool> entry in _partPresetFilterToggleMap)
                {
                    if (entry.Value)
                    {
                        if (_sidekickRuntime.MappedPresetFilterDictionary.TryGetValue(entry.Key.Term, out List<SidekickPartPreset> presets))
                        {
                            mappedPresets.UnionWith(presets);
                        }
                    }
                }

                if (_sidekickRuntime.MappedBasePresetDictionary.TryGetValue(_currentSpecies, out List<SidekickPartPreset> basePresets))
                {
                    mappedPresets.UnionWith(basePresets);
                }
            }
            else
            {
                mappedPresets.UnionWith(_availablePresets);
            }

            List<SidekickPartPreset> allPresets = mappedPresets.ToList();

            SidekickSpecies unrestrictedSpecies = SidekickSpecies.GetByName(_dbManager, "Unrestricted");

            foreach (PartGroup partGroup in Enum.GetValues(typeof(PartGroup)))
            {
                // only filter head part presets by species
                List<SidekickPartPreset> presets = partGroup is PartGroup.Head && _currentSpecies.ID != unrestrictedSpecies?.ID
                    ? allPresets.Where(preset => preset.Species.ID == _currentSpecies.ID && preset.PartGroup == partGroup).ToList()
                    : allPresets.Where(preset => preset.PartGroup == partGroup).ToList();
                List<string> presetNames = new List<string>();
                foreach (SidekickPartPreset preset in presets)
                {
                    switch (partGroup)
                    {
                        case PartGroup.Head:
                            _currentHeadPresetDictionary.Add(preset.Name, preset);
                            tooltipText = "Select a head preset for you character - a head preset is made up of parts like a head, nose, eyes and teeth etc.";
                            break;
                        case PartGroup.UpperBody:
                            _currentUpperBodyPresetDictionary.Add(preset.Name, preset);
                            tooltipText = "Select an upper body preset for you character - an upper body preset is made up of parts like a torso, arms, hands and a back attachment etc.";
                            break;
                        case PartGroup.LowerBody:
                            _currentLowerBodyPresetDictionary.Add(preset.Name, preset);
                            tooltipText = "Select a lower body preset for you character - a lower body preset is made up of parts like hips, legs, feet and hip attachments etc.";
                            break;
                    }

                    presetNames.Add(preset.Name);
                }

                string defaultValue = "None";

                if (presetNames.Count > 0)
                {
                    if (_presetDefaultValues.TryGetValue(partGroup.ToString(), out string value))
                    {
                        defaultValue = presetNames.Contains(value) ? value : "None";

                        if (_processingSpeciesChange && partGroup == PartGroup.Head && value != "None" && defaultValue == "None")
                        {
                            defaultValue = presetNames[Random.Range(0, presetNames.Count - 1)];
                        }
                    }
                }

                presetNames.Sort();

                dropdowns[partGroup.ToString()] = CreatePresetRow(_presetPartContainer, partGroup.ToString(), tooltipText, presetNames, true, defaultValue, PresetDropdownType.Part);
            }
        }

        /// <summary>
        ///     Create a selection row for the preset tab with the given values.
        /// </summary>
        /// <param name="view">The view to add the row to.</param>
        /// <param name="rowLabel">The label to put in the row.</param>
        /// <param name="tooltipText">The tooltip to display for this row.</param>
        /// <param name="dropdownValues">The values for the dropdown in the row.</param>
        /// <param name="includeNoneValue">Whether to include `None` as a value in the dropdown.</param>
        /// <param name="defaultValue">The default value to select from the dropdown.</param>
        /// <param name="dropdownType">What section of the preset UI this dropdown is part of.</param>
        /// <returns>The dropdown selection UI element.</returns>
        private PopupField<string> CreatePresetRow(
            VisualElement view,
            string rowLabel,
            string tooltipText,
            List<string> dropdownValues,
            bool includeNoneValue,
            string defaultValue,
            PresetDropdownType dropdownType)
        {
            VisualElement partContainer = new VisualElement
            {
                style =
                {
                    minHeight = 20,
                    display = DisplayStyle.Flex,
                    flexDirection = FlexDirection.Row,
                    marginBottom = 2,
                    marginTop = 2,
                    marginLeft = 15,
                    marginRight = 2,
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                }
            };

            Label partTypeTitle = new Label(rowLabel.ToString())
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    width = 150
                },
                tooltip = tooltipText
            };

            Button removeButton = new Button()
            {
                tooltip = "Remove this preset, resets selection to None"
            };

            removeButton.Add(
                new Image
                {
                    image = Resources.Load<Texture2D>("UI/T_Clear"),
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            int marginLeft = 0;
            if (dropdownType != PresetDropdownType.Part)
            {
                marginLeft = 36;
            }

            Button previousButton = new Button()
            {
                tooltip = "Select the previous preset",
                style =
                {
                    marginLeft = marginLeft
                }
            };

            previousButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_prev", "|Previous Preset").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button nextButton = new Button()
            {
                tooltip = "Select the next preset"
            };

            nextButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_next", "|Next Preset").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button randomButton = new Button()
            {
                tooltip = "Randomly select a preset"
            };

            Texture2D randomImage = Resources.Load<Texture2D>("UI/T_Random");
            randomButton.Add(
                new Image
                {
                    image = randomImage,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingTop = new StyleLength(1),
                        paddingBottom = new StyleLength(1)
                    }
                }
            );

            if (!dropdownValues.Contains(defaultValue))
            {
                defaultValue = includeNoneValue ? "None" : dropdownValues[0];
            }

            List<string> popupValues = new List<string>();
            if (includeNoneValue)
            {
                popupValues.Add("None");
            };

            popupValues.AddRange(dropdownValues);

            PopupField<string> partSelection = new PopupField<string>(popupValues, 0)
            {
                value = "None",
                style =
                {
                    minWidth = 180,
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 20
                },
                tooltip = tooltipText
            };
            ScrollableDropdown.Apply(partSelection);

            partSelection.RegisterValueChangedCallback(
                evt =>
                {
                    _presetDefaultValues[rowLabel] = evt.newValue;

                    // Correctly enable/disable next and previous buttons based on selection
                    previousButton.SetEnabled(partSelection.index > 0);
                    nextButton.SetEnabled(partSelection.index < popupValues.Count - 1);

                    switch (dropdownType)
                    {
                        case PresetDropdownType.Part:
                            RecordUndo("Sidekick Part Preset");
                            _applyingPreset = true;

                            bool hasErrors = false;
                            string errorMessage = "The following parts could not be found in your project:\n";
                            Enum.TryParse(rowLabel, out PartGroup group);
                            List<CharacterPartType> partTypesToRemove = group.GetPartTypes();
                            SidekickPartPreset currentPartPreset = null;
                            List<SidekickPartPresetRow> presetParts = new List<SidekickPartPresetRow>();
                            // NOTE : need to ensure evt.newValue is always in the dictionary ahead of this, or change to GetValueOrDefault()
                            if (evt.newValue != "None")
                            {
                                switch (group)
                                {
                                    case PartGroup.Head:
                                        currentPartPreset = _currentHeadPresetDictionary[evt.newValue];
                                        break;
                                    case PartGroup.UpperBody:
                                        currentPartPreset = _currentUpperBodyPresetDictionary[evt.newValue];
                                        break;
                                    case PartGroup.LowerBody:
                                        currentPartPreset = _currentLowerBodyPresetDictionary[evt.newValue];
                                        break;
                                }
                                presetParts = SidekickPartPresetRow.GetAllByPreset(_dbManager, currentPartPreset);
                            }

                            foreach (SidekickPartPresetRow presetPart in presetParts)
                            {
                                if (Enum.TryParse(CharacterPartTypeUtils.GetTypeNameFromShortcode(presetPart.PartType), out CharacterPartType partType))
                                {
                                    if (_partSelectionDictionary.TryGetValue(partType, out PartTypeControls currentField))
                                    {
                                        if (presetPart.Part != null)
                                        {
                                            _currentCharacter[partType] = presetPart.Part;
                                        }

                                        UpdateResult result = new UpdateResult(errorMessage, hasErrors);

                                        if (partType == CharacterPartType.Wrap)
                                        {
                                            SyncWrapToBodyType();
                                        }
                                        else
                                        {
                                            result = UpdatePartDropdown(
                                                currentField,
                                                presetPart.Part?.Name ?? "None",
                                                errorMessage,
                                                hasErrors
                                            );
                                        }

                                        hasErrors = result.HasErrors;
                                        errorMessage = result.ErrorMessage;
                                        partTypesToRemove.Remove(partType);
                                    }
                                }
                            }

                            foreach (CharacterPartType partType in partTypesToRemove)
                            {
                                if (_partSelectionDictionary.TryGetValue(partType, out PartTypeControls currentField))
                                {
                                    UpdateResult result = UpdatePartDropdown(currentField, "None", errorMessage, hasErrors);

                                    hasErrors = result.HasErrors;
                                    errorMessage = result.ErrorMessage;
                                }
                            }

                            if (hasErrors)
                            {
                                EditorUtility.DisplayDialog(
                                    "Assets Missing",
                                    errorMessage,
                                    "OK"
                                );
                            }

                            SchedulePreviewRegeneration();
                            break;
                        case PresetDropdownType.Body:
                            RecordUndo("Sidekick Body Preset");
                            SidekickBodyShapePreset bodyShapePreset = _currentBodyPresetDictionary[evt.newValue];
                            if (Math.Abs(_bodyTypeSlider.value - bodyShapePreset.BodyType) < 0.001f)
                            {
                                // The slider value is not changing, so its callback will not run; sync the wrap here.
                                SyncWrapToBodyType();
                            }

                            _bodyTypeSlider.value = bodyShapePreset.BodyType;
                            _bodySizeSlider.value = bodyShapePreset.BodySize;
                            _musclesSlider.value = bodyShapePreset.Musculature;
                            CommitUndoState();
                            break;
                        case PresetDropdownType.Color:
                            if (evt.newValue == "None")
                            {
                                return;
                            }

                            RecordUndo("Sidekick Color Preset");
                            ResetCurrentColorSet();

                            Enum.TryParse(rowLabel, out ColorGroup colorGroup);
                            SidekickColorPreset colorPreset = null;
                            // NOTE : need to ensure evt.newValue is always in the dictionary ahead of this, or change to GetValueOrDefault()
                            switch (colorGroup)
                            {
                                case ColorGroup.Species:
                                    colorPreset = _currentColorSpeciesPresetDictionary[evt.newValue];
                                    break;
                                case ColorGroup.Outfits:
                                    colorPreset = _currentColorOutfitsPresetDictionary[evt.newValue];
                                    break;
                                case ColorGroup.Attachments:
                                    colorPreset = _currentColorAttachmentsPresetDictionary[evt.newValue];
                                    break;
                                case ColorGroup.Materials:
                                    colorPreset = _currentColorMaterialsPresetDictionary[evt.newValue];
                                    break;/*
                                case ColorGroup.Elements:
                                    colorPreset = _currentColorElementsPresetDictionary[evt.newValue];
                                    break;*/
                            }

                            List<SidekickColorPresetRow> presetColorRows = SidekickColorPresetRow.GetAllByPreset(_dbManager, colorPreset);
                            foreach (SidekickColorPresetRow row in presetColorRows)
                            {
                                SidekickColorRow existingRow = _allColorRows.Find(r => r.ColorProperty.ID == row.ColorProperty.ID);
                                if (existingRow == null)
                                {
                                    existingRow = new SidekickColorRow()
                                    {
                                        ID = -1,
                                        ColorSet = _currentColorSet,
                                        ColorProperty = row.ColorProperty,
                                        NiceColor = row.NiceColor,
                                        NiceMetallic = row.NiceMetallic,
                                        NiceSmoothness = row.NiceSmoothness,
                                        NiceReflection = row.NiceReflection,
                                        NiceEmission = row.NiceEmission,
                                        NiceOpacity = row.NiceOpacity
                                    };

                                    _allColorRows.Add(existingRow);
                                }
                                else
                                {
                                    existingRow.NiceColor = row.NiceColor;
                                    existingRow.NiceMetallic = row.NiceMetallic;
                                    existingRow.NiceSmoothness = row.NiceSmoothness;
                                    existingRow.NiceReflection = row.NiceReflection;
                                    existingRow.NiceEmission = row.NiceEmission;
                                    existingRow.NiceOpacity = row.NiceOpacity;
                                }

                                // During a batch operation, defer the in-place texture write to the single rebuild
                                // so colours don't apply as a separate visible stage before the parts swap in.
                                if (_applyingPreset)
                                {
                                    _reapplyColorsOnRegen = true;
                                }
                                else
                                {
                                    UpdateAllColors(existingRow);
                                }
                            }

                            PopulatePartColorRows();
                            RefreshVisibleColorRows();
                            // No-ops when reached mid-batch (e.g. from Randomize Character); the batch's single
                            // rebuild commits instead.
                            CommitUndoState();
                            break;
                        case PresetDropdownType.Texture:
                            // TODO: Add texture setting functionality once decal system in place
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dropdownType), dropdownType, null);
                    }
                }
            );

            removeButton.clickable.clicked += delegate
            {
                // TODO: Change to default character when available
                partSelection.value = "None";
            };

            previousButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index - 1;
                if (newIndex <= 0)
                {
                    newIndex = 0;
                }

                partSelection.index = newIndex;
            };

            nextButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index + 1;
                if (newIndex >= partSelection.choices.Count - 1)
                {
                    newIndex = partSelection.choices.Count - 1;
                }

                partSelection.index = newIndex;
            };

            randomButton.clickable.clicked += delegate
            {
                int currentIndex = partSelection.index;
                if (partSelection.choices.Count - 1 > 1)
                {
                    while (partSelection.index == currentIndex)
                    {
                        partSelection.index = Random.Range(1, partSelection.choices.Count);
                    }
                }
            };

            partContainer.Add(partTypeTitle);
            if (dropdownType == PresetDropdownType.Part)
            {
                partContainer.Add(removeButton);
            }
            partContainer.Add(previousButton);
            partContainer.Add(nextButton);
            partContainer.Add(randomButton);
            partContainer.Add(partSelection);
            view.Add(partContainer);

            // Set without notifying: rebuilding the UI must restore what the dropdown shows without re-applying the preset
            // (which would overwrite slider values and parts that were changed after the preset was selected).
            partSelection.SetValueWithoutNotify(defaultValue);
            _presetDefaultValues[rowLabel] = defaultValue;
            previousButton.SetEnabled(partSelection.index > 0);
            nextButton.SetEnabled(partSelection.index < popupValues.Count - 1);

            _presetRowControls[rowLabel] = new PresetRowControls
            {
                Dropdown = partSelection,
                PreviousButton = previousButton,
                NextButton = nextButton,
                PopupValues = popupValues,
                IncludeNoneValue = includeNoneValue
            };

            return partSelection;
        }

        /// <summary>
        ///     Updates the required parts of the UI on a species change.
        /// </summary>
        /// <param name="newSpecies">The name of the species being changed to.</param>
        private void ProcessSpeciesChange(string newSpecies)
        {
            // don't need to re-process if multiple callbacks are triggered
            if (_currentSpecies.Name == newSpecies)
            {
                return;
            }

            // No-ops when reached from loads/undo (their suppression flags are already raised); the deferred
            // dropdown cascade coalesces into this one undo group.
            RecordUndo("Sidekick Species Change");

            _currentSpecies = _allSpecies.FirstOrDefault(species => species.Name == newSpecies);
            _sidekickRuntime.CurrentSpecies = _currentSpecies;
            UpdateVisibleColorSets();
            ResetCurrentColorSet();
            if (_allColorRows.Count == 0)
            {
                PopulateColorRowsFromTextures();
            }

            _appliedPartFilters.ResetFiltersForSpeciesChange();

            _processingSpeciesChange = true;
            UpdatePartDropdowns();
            PopulatePresetUI();
            _processingSpeciesChange = false;
        }

        /// <summary>
        ///     Populates the parts UI.
        /// </summary>
        private void PopulatePartUI()
        {
            _partView.Clear();

            _availablePartList = new List<SidekickPart>();
            _partSelectionDictionary = new Dictionary<CharacterPartType, PartTypeControls>();
            _partLockMap = new Dictionary<PopupField<string>, bool>();

            Foldout speciesFoldout = new Foldout
            {
                text = "Select - Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            List<string> speciesNames = _allSpecies.Select(species => species.Name).ToList();

            _speciesField = new DropdownField
            {
                label = "Species",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                },
                tooltip = "Select the species of your character"
            };
            _speciesField.choices = speciesNames;
            ScrollableDropdown.Apply(_speciesField);
            _speciesField.RegisterValueChangedCallback(
                evt =>
                {
                    _speciesPresetField.value = _speciesField.value;
                    ProcessSpeciesChange(evt.newValue);
                }
            );

            _speciesField.index = _currentSpecies != null && speciesNames.Count > 0 ? _speciesField.choices.IndexOf(_currentSpecies.Name) : 0;
            speciesFoldout.Add(_speciesField);
            _partView.Add(speciesFoldout);

            _partsFoldout = new Foldout()
            {
                text = "Select - Parts",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            List<SidekickPartFilter> outfitFilters = SidekickPartFilter.GetAllForFilterType(_dbManager, FilterType.Outfit);
            List<SidekickPartFilter> orderedFilters = new List<SidekickPartFilter>();

            outfitFilters.Sort(
                (filterA, filterB) => String.CompareOrdinal(filterA.Term, filterB.Term)
            );

            orderedFilters.AddRange(outfitFilters);

            Foldout filterFoldout = new Foldout()
            {
                text = "Select - Outfit Filter",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };

            _appliedPartFilters = new FilterGroup()
            {
                Runtime = _sidekickRuntime,
                CombineType = FilterCombineType.Or
            };

            VisualElement toggleList = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
                }
            };

            List<Toggle> allFilterToggles = new List<Toggle>();

            Color borderColor = new Color(0.17f, 0.17f, 0.17f);
            Color backgroundColor = new Color(0.35f, 0.35f, 0.35f);

            foreach (SidekickPartFilter filter in orderedFilters)
            {
                Toggle outfitToggle = new Toggle(filter.Term)
                {
                    value = true,
                    style =
                    {
                        width = 160,
                        borderBottomWidth = 1,
                        borderBottomColor = borderColor,
                        paddingBottom = 2,
                        borderLeftWidth = 1,
                        borderLeftColor = borderColor,
                        paddingLeft = 2,
                        borderRightWidth = 1,
                        borderRightColor = borderColor,
                        paddingRight = 2,
                        borderTopWidth = 1,
                        borderTopColor = borderColor,
                        paddingTop = 2,
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3,
                        backgroundColor = backgroundColor,
                        textOverflow = new StyleEnum<TextOverflow>(TextOverflow.Ellipsis)
                    }
                };

                FilterItem filterItem = new FilterItem(_sidekickRuntime, filter, FilterCombineType.Or);
                _appliedPartFilters.AddFilterItem(filterItem);

                outfitToggle.RegisterValueChangedCallback(
                    evt =>
                    {
                        if (evt.newValue)
                        {
                            _appliedPartFilters.AddFilterItem(filterItem);
                        }
                        else
                        {
                            _appliedPartFilters.RemoveFilterItem(filterItem);
                        }
                        UpdatePartDropdowns();
                    }
                );

                toggleList.Add(outfitToggle);
                allFilterToggles.Add(outfitToggle);
            }

            VisualElement buttonRow = new VisualElement()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            Button selectAll = new Button(
                delegate
                {
                    foreach (Toggle toggle in allFilterToggles)
                    {
                        toggle.value = true;
                    }
                }
            )
            {
                text = "Select All"
            };

            Button selectNone = new Button(
                delegate
                {
                    foreach (Toggle toggle in allFilterToggles)
                    {
                        toggle.value = false;
                    }
                }
            )
            {
                text = "Select None"
            };

            buttonRow.Add(selectAll);
            buttonRow.Add(selectNone);

            filterFoldout.Add(buttonRow);
            filterFoldout.Add(toggleList);

            _partView.Add(filterFoldout);

            foreach (PartGroup partGroup in Enum.GetValues(typeof(PartGroup)))
            {
                string labelText = StringUtils.AddSpacesBeforeCapitalLetters(partGroup.ToString());
                Foldout partGroupFoldout = new Foldout
                {
                    text = labelText,
                    style =
                    {
                        marginLeft = 15,
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                    }
                };

                Button randomiseAllButton = new Button()
                {
                    text = "Randomize " + labelText
                };

                partGroupFoldout.Add(randomiseAllButton);

                foreach (CharacterPartType value in partGroup.GetPartTypes())
                {
                    VisualElement partContainer = new VisualElement
                    {
                        style =
                        {
                            minHeight = 20,
                            display = DisplayStyle.Flex,
                            flexDirection = FlexDirection.Row,
                            marginBottom = 2,
                            marginTop = 2,
                            marginRight = 2,
                            unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Normal)
                        }
                    };
                    BuildPartDetails(value, partContainer);
                    partGroupFoldout.Add(partContainer);
                }

                _partsFoldout.Add(partGroupFoldout);

                randomiseAllButton.clickable.clicked += delegate
                {
                    if (_applyingPreset)
                    {
                        return;
                    }

                    RecordUndo("Randomize " + partGroup);

                    // Batch the whole group into a single rebuild (and a single undo step), the same way the
                    // Randomize Character button does.
                    _applyingPreset = true;
                    foreach (CharacterPartType value in partGroup.GetPartTypes())
                    {
                        PartTypeControls dropdown = _partSelectionDictionary[value];
                        bool locked = _partLockMap[dropdown.PartDropdown];
                        if (!locked)
                        {
                            if (dropdown.PartDropdown.choices.Count > 1)
                            {
                                dropdown.RandomisePartDropdownValue();
                            }
                            else
                            {
                                dropdown.SetPartDropdownValue("None");
                            }
                        }
                    }

                    SchedulePreviewRegeneration();
                };
            }

            UpdatePartDropdowns();

            _partView.Add(_partsFoldout);
        }

        /// <summary>
        ///     Builds the UI details for each different part type.
        /// </summary>
        /// <param name="type">The type of the part to build the UI for.</param>
        /// <param name="partContainer">The container to add the UI to.</param>
        private void BuildPartDetails(CharacterPartType type, VisualElement partContainer)
        {
            List<SidekickPart> partsList = new List<SidekickPart>();

            foreach (SidekickPart part in _allPartsLibrary[type])
            {
                if (_availablePartList.Any(p => p.ID == part.ID))
                {
                    partsList.Add(part);
                }
            }

            Label partTypeTitle = new Label(type.ToString())
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    width = 150
                },
                tooltip = type.GetTooltipForPartType()
            };

            Image lockImage = new Image
            {
                image = EditorGUIUtility.IconContent("LockIcon", "|Lock Part").image,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    alignSelf = new StyleEnum<Align>(Align.Center),
                    width = 15,
                    height = 15,
                    paddingTop = 2
                }
            };

            Button lockButton = new Button()
            {
                tooltip = "Remove this part"
            };

            lockButton.Add(
                lockImage
            );

            Button removeButton = new Button()
            {
                tooltip = "Remove this part"
            };

            removeButton.Add(
                new Image
                {
                    image = Resources.Load<Texture2D>("UI/T_Clear"),
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button previousButton = new Button()
            {
                tooltip = "Select the previous part"
            };

            previousButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_prev", "|Previous Part").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button nextButton = new Button()
            {
                tooltip = "Select the next part"
            };

            nextButton.Add(
                new Image
                {
                    image = EditorGUIUtility.IconContent("tab_next", "|Next Part").image,
                    scaleMode = ScaleMode.ScaleToFit
                }
            );

            Button randomButton = new Button()
            {
                tooltip = "Randomly select a part"
            };

            Texture2D randomImage = Resources.Load<Texture2D>("UI/T_Random");
            randomButton.Add(
                new Image
                {
                    image = randomImage,
                    scaleMode = ScaleMode.ScaleToFit,
                    style =
                    {
                        paddingTop = new StyleLength(1),
                        paddingBottom = new StyleLength(1)
                    }
                }
            );

            List<string> popupValues = new List<string>();

            foreach (SidekickPart part in partsList)
            {
                SidekickPartSpeciesLink link = SidekickPartSpeciesLink.GetForSpeciesAndPart(_dbManager, _currentSpecies, part);
                if (link != null)
                {
                    popupValues.Add(part.Name);
                }
            }

            popupValues.Sort(StringComparer.OrdinalIgnoreCase);

            if (type != CharacterPartType.Wrap)
            {
                popupValues.Insert(0, "None");
            }

            _currentCharacter.TryGetValue(type, out SidekickPart selectedPart);
            string currentSelection = selectedPart?.Name ?? "None";
            if (_processingSpeciesChange)
            {
                if (popupValues.Count < 1 || (popupValues.Count == 1 && popupValues[0] == "None"))
                {
                    _previousPartSelections[type] = currentSelection;
                    currentSelection = "None";
                }
                else if (PartUtils.IsBaseSpeciesPart(currentSelection))
                {
                    _previousPartSelections[type] = currentSelection;
                    currentSelection = popupValues.Find(n => n.Contains("BASE")) ?? "None";
                }
                else if (currentSelection == "None" && _previousPartSelections[type] != "None" && popupValues.Count > 1)
                {
                    currentSelection = _previousPartSelections[type];
                    _previousPartSelections[type] = "None";
                }

                if (!popupValues.Contains(currentSelection))
                {
                    _previousPartSelections[type] = currentSelection;
                    currentSelection = FindClosestPartMatch(popupValues, currentSelection);
                }
            }

            PopupField<string> partSelection = new PopupField<string>(popupValues, 0)
            {
                value = currentSelection,
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    marginRight = 15
                }
            };
            ScrollableDropdown.Apply(partSelection);

            PartTypeControls controls = new PartTypeControls
            {
                PartType = type,
                PartDropdown = partSelection,
                ClearButton = removeButton,
                NextButton = nextButton,
                PreviousButton = previousButton,
                RandomButton = randomButton
            };

            partSelection.RegisterCallback<ChangeEvent<string>>(
                changeEvent =>
                {
                    PartSelectionChangeEvent(changeEvent, type, controls);
                }
            );

            _partLockMap[partSelection] = false;

            lockButton.clickable.clicked += delegate
            {
                bool newValue = !_partLockMap[partSelection];
                _partLockMap[partSelection] = newValue;

                if (newValue)
                {
                    partSelection.SetEnabled(false);
                    removeButton.SetEnabled(false);
                    nextButton.SetEnabled(false);
                    previousButton.SetEnabled(false);
                    randomButton.SetEnabled(false);
                    lockImage.image = EditorGUIUtility.IconContent("LockIcon-On", "|Unlock Part").image;
                    lockButton.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
                }
                else
                {
                    partSelection.SetEnabled(true);
                    removeButton.SetEnabled(true);
                    nextButton.SetEnabled(true);
                    previousButton.SetEnabled(true);
                    randomButton.SetEnabled(true);
                    PartSelectionChangeEvent(new ChangeEvent<string>(), type, controls);
                    lockImage.image = EditorGUIUtility.IconContent("LockIcon", "|Lock Part").image;
                    lockButton.style.backgroundColor = new Color(0.345098f, 0.345098f, 0.345098f);
                }
            };

            if (_processingSpeciesChange)
            {
                ChangeEvent<string> changeEvent = ChangeEvent<string>.GetPooled(_previousPartSelections[type], currentSelection);
                PartSelectionChangeEvent(changeEvent, type, controls);
                changeEvent.Dispose();
            }

            _partSelectionDictionary.Add(type, controls);

            removeButton.clickable.clicked += delegate
            {
                partSelection.value = "None";
            };

            previousButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index - 1;
                if (newIndex <= 0)
                {
                    newIndex = 0;
                }

                partSelection.index = newIndex;
            };

            nextButton.clickable.clicked += delegate
            {
                int newIndex = partSelection.index + 1;
                if (newIndex >= partSelection.choices.Count - 1)
                {
                    newIndex = partSelection.choices.Count - 1;
                }

                partSelection.index = newIndex;
            };

            randomButton.clickable.clicked += delegate
            {
                int currentIndex = partSelection.index;
                if (partSelection.choices.Count - 1 > 1)
                {
                    while (partSelection.index == currentIndex)
                    {
                        partSelection.index = Random.Range(1, partSelection.choices.Count);
                    }
                }
            };

            partContainer.Add(partTypeTitle);
            partContainer.Add(lockButton);
            partContainer.Add(removeButton);
            partContainer.Add(previousButton);
            partContainer.Add(nextButton);
            partContainer.Add(randomButton);
            partContainer.Add(partSelection);
        }

        private void UpdatePartDropdowns()
        {
            Dictionary<CharacterPartType, List<string>> filteredParts = _appliedPartFilters.GetFilteredParts();
            Dictionary<CharacterPartType, List<string>> baseParts = _sidekickRuntime.MappedBasePartDictionary[_currentSpecies];

            foreach (CharacterPartType type in Enum.GetValues(typeof(CharacterPartType)))
            {
                PartTypeControls controls = _partSelectionDictionary[type];
                HashSet<string> popupItems = new HashSet<string>();

                HashSet<string> itemList = baseParts.TryGetValue(type, out List<string> items) ? items.ToHashSet() : new HashSet<string>();
                popupItems.UnionWith(itemList);
                itemList = filteredParts.TryGetValue(type, out List<string> filteredItems) ? filteredItems.ToHashSet() : new HashSet<string>();
                popupItems.UnionWith(itemList);

                List<string> popupValues = popupItems.ToList();
                popupValues.Sort(StringComparer.OrdinalIgnoreCase);

                if (type != CharacterPartType.Wrap)
                {
                    popupValues.Insert(0, "None");
                }

                _currentCharacter.TryGetValue(type, out SidekickPart selectedPart);
                string currentSelection = selectedPart?.Name ?? "None";
                if (_processingSpeciesChange)
                {
                    if (popupValues.Count < 1 || (popupValues.Count == 1 && popupValues[0] == "None"))
                    {
                        _previousPartSelections[type] = currentSelection;
                        currentSelection = "None";
                    }
                    else if (PartUtils.IsBaseSpeciesPart(currentSelection))
                    {
                        _previousPartSelections[type] = currentSelection;
                        currentSelection = popupValues.Find(n => n.Contains("BASE")) ?? "None";
                    }
                    else if (currentSelection == "None" && _previousPartSelections[type] != "None" && popupValues.Count > 1)
                    {
                        currentSelection = _previousPartSelections[type];
                        _previousPartSelections[type] = "None";
                    }

                    if (!popupValues.Contains(currentSelection))
                    {
                        _previousPartSelections[type] = currentSelection;
                        currentSelection = FindClosestPartMatch(popupValues, currentSelection);
                    }
                }

                if (type == CharacterPartType.Wrap && currentSelection == "None")
                {
                    currentSelection = null;
                }

                controls.UpdateDropdownValues(popupValues);
                controls.SetPartDropdownValue(currentSelection);
                controls.UpdateControls();
            }
        }

        /// <summary>
        ///     Process the change event when a new part is selected.
        /// </summary>
        /// <param name="changeEvent">The change event to process.</param>
        /// <param name="type">The type of part that has been changed.</param>
        /// <param name="partSelection">The UI PopupField the change event is happening for.</param>
        private void PartSelectionChangeEvent(ChangeEvent<string> changeEvent, CharacterPartType type, PartTypeControls partSelection)
        {
            try
            {
                RecordUndo("Sidekick Part Change");

                if (_currentTab == TabView.Parts
                    && _sidekickRuntime.MappedPartDictionary.ContainsKey(type)
                    && changeEvent.newValue != null
                    && _sidekickRuntime.MappedPartDictionary[type].TryGetValue(changeEvent.newValue, out SidekickPart selectedPart))
                {

                    GameObject partModel = selectedPart.GetPartModel();
                    SkinnedMeshRenderer selectedMesh = partModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    _partDictionary[type] = selectedMesh;
                    _currentCharacter[type] = selectedPart;

                    if (!_processingSpeciesChange)
                    {
                        _previousPartSelections[type] = changeEvent.newValue;
                    }

                    if (type == CharacterPartType.Wrap)
                    {
                        // Remembered so the same wrap comes back when the body type re-crosses the threshold.
                        _lastSelectedWrap = changeEvent.newValue;
                    }

                    if (type == CharacterPartType.Torso)
                    {
                        _requiresWrap = selectedPart.UsesWrap;
                        SyncWrapToBodyType();
                    }
                }
                else if (changeEvent.newValue == "None")
                {
                    _currentCharacter.Remove(type);
                    _partDictionary.Remove(type);
                    if (!_processingSpeciesChange)
                    {
                        _previousPartSelections[type] = "None";
                    }

                    if (type == CharacterPartType.Torso)
                    {
                        SyncWrapToBodyType();
                    }
                }
                else
                {
                    _partDictionary.Remove(type);
                }

                if (!_applyingPreset && _previewToggle.value)
                {
                    RegeneratePreviewCharacter(false, true);
                    bool switchAnimation = SetupAnimationControllers();

                    if (switchAnimation && (type == CharacterPartType.HandLeft || type == CharacterPartType.HandRight))
                    {
                        SetState("InspectHands");
                    }

                    UpdatePartUVData();
                }

                partSelection.UpdateControls();

                // Covers the preview-toggle-off path; with the preview on, RegeneratePreviewCharacter commits.
                CommitUndoState();
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Failed loading part", "Failed to load the following part\n" + changeEvent.newValue, "OK");
                Debug.LogWarning(ex);
            }
        }

        /// <summary>
        ///     Selects the first match, with the highest number of matching terms from a list of parts.
        /// </summary>
        /// <param name="availableParts">The list of parts.</param>
        /// <param name="existingPart">The part to find the closest match for.</param>
        /// <returns>The part with the closest match to the given part.</returns>
        private string FindClosestPartMatch(List<string> availableParts, string existingPart)
        {
            string closestMatch = "None";
            List<string> partSections = existingPart.Split("_").ToList();
            Dictionary<string, int> matchCounts = new Dictionary<string, int>();
            foreach (string section in partSections)
            {
                foreach (string part in availableParts)
                {
                    if (part.Contains(section))
                    {
                        if (matchCounts.TryGetValue(part, out int count))
                        {
                            matchCounts[part] = count + 1;
                        }
                        else
                        {
                            matchCounts[part] = 1;
                        }
                    }
                }
            }

            int currentMax = 0;

            foreach (KeyValuePair<string, int> match in matchCounts)
            {
                if (match.Value > currentMax)
                {
                    closestMatch = match.Key;
                    currentMax = match.Value;
                }
            }

            return closestMatch;
        }

        /// <summary>
        ///     Generates a character from the current selected parts.
        /// </summary>
        private GameObject GenerateCharacter(bool combineMesh, bool processBoneMovement)
        {
            List<SkinnedMeshRenderer> parts = new List<SkinnedMeshRenderer>();
            foreach (KeyValuePair<CharacterPartType, SidekickPart> entry in _currentCharacter)
            {
                if (entry.Value != null)
                {
                    // Only apply wrap when required
                    if (entry.Key == CharacterPartType.Wrap && (!_requiresWrap || _bodyTypeBlendValue <= 0))
                    {
                        continue;
                    }

                    GameObject partModel = entry.Value.GetPartModel();

                    if (partModel == null)
                    {
                        if (_showMissingPartsPopup)
                        {
                            EditorUtility.DisplayDialog(
                                "Error loading part",
                                "Unable to load part: " + entry.Value.Name + ".",
                                "Ok"
                            );
                        }
                        continue;
                    }

                    SkinnedMeshRenderer selectedMesh = partModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (selectedMesh != null)
                    {
                        parts.Add(selectedMesh);
                    }
                }
            }

            GameObject newModel = null;
            try
            {
                newModel = _sidekickRuntime.CreateCharacter(
                    _OUTPUT_MODEL_NAME,
                    parts,
                    combineMesh,
                    processBoneMovement,
                    null,
                    _combineBodyBlendShapes,
                    _combineFacialBlendShapes
                );
                _currentAnimator = null;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Error creating character",
                    "Something went wrong when creating the character.\nPlease try again.",
                    "Ok"
                );
                Debug.LogWarning(ex);
            }

            return newModel;
        }

        /// <summary>
        ///     Destroys the current preview character and generates a new one from the current selected parts.
        /// </summary>
        private void RegeneratePreviewCharacter(bool combineMesh, bool processBoneMovement)
        {
            if (_newModel != null)
            {
                DestroyImmediate(_newModel);
            }

            // Also remove any stale duplicates left in the scene (e.g. from older sessions) so only the new model remains.
            GameObject existingModel = GameObject.Find(_OUTPUT_MODEL_NAME);
            while (existingModel != null)
            {
                DestroyImmediate(existingModel);
                existingModel = GameObject.Find(_OUTPUT_MODEL_NAME);
            }

            _newModel = GenerateCharacter(combineMesh, processBoneMovement);

            // Keep the saved character snapshot in sync with the last built character, so entering play mode restores the
            // right character even if the window has been rebuilt (e.g. by a script recompile) before play is pressed.
            if (!EditorApplication.isPlaying
                && _newModel != null
                && _currentSpecies != null
                && _currentColorSet != null
                && _currentCharacter.Count > 0)
            {
                SerializedCharacter savedCharacter = CreateSerializedCharacter(_OUTPUT_MODEL_NAME);
                Serializer serializer = new Serializer();
                EditorPrefs.SetString(_AUTOSAVE_KEY, serializer.Serialize(savedCharacter));
            }

            // The universal undo settle point: every immediate part rebuild and every debounced batch rebuild
            // (presets, loads, undo application) passes through here with its state fully applied. Kept outside
            // the autosave guard so clearing the last part still commits.
            CommitUndoState();
        }

        /// <summary>
        ///     Schedules a single preview regeneration for after the UI event queue has been processed. Dropdown value changes
        ///     made while applying a preset or loading a character dispatch their change events after the current event finishes,
        ///     so the regeneration (and re-enabling of the part change regeneration) must happen after those have run.
        /// </summary>
        private void SchedulePreviewRegeneration()
        {
            _applyingPreset = true;

            // Every request pushes back the debounce deadline. The deferred dropdown change-event cascade
            // (presets -> parts -> wrap) can span several editor ticks with gaps, so we cannot rely on a
            // single "quiet tick" - that rebuilds prematurely with partial parts and then again with the
            // final parts. Instead we wait until requests have stopped arriving for the debounce window.
            // This also keeps _applyingPreset true for the whole cascade, so the in-place body/colour
            // suppression holds until the single rebuild applies everything together.
            _lastRegenRequestTime = EditorApplication.timeSinceStartup;
            if (_regenerateQueued)
            {
                return;
            }

            _regenerateQueued = true;
            EditorApplication.update += DebouncedRegenerationTick;
        }

        /// <summary>
        ///     Polls until the regeneration request stream has been quiet for <see cref="_REGEN_DEBOUNCE_SECONDS" />, then
        ///     rebuilds the preview exactly once with the fully settled character state. Driven by
        ///     <see cref="SchedulePreviewRegeneration" />.
        /// </summary>
        private void DebouncedRegenerationTick()
        {
            if (EditorApplication.timeSinceStartup - _lastRegenRequestTime < _REGEN_DEBOUNCE_SECONDS)
            {
                return;
            }

            EditorApplication.update -= DebouncedRegenerationTick;
            _regenerateQueued = false;
            _applyingPreset = false;
            RegeneratePreviewCharacter(_combineMeshes, true);
            UpdatePartUVData();

            // Apply colours deferred during the batch (the rebuild itself does not apply colours), so parts,
            // body shape and colours all appear in this single update. Every row is painted, not just the
            // visible ones: the colour map is shared across rebuilds, so cells belonging to parts that are not
            // equipped right now must also hold the loaded/preset colours for when those parts are chosen later.
            if (_reapplyColorsOnRegen)
            {
                _reapplyColorsOnRegen = false;
                UpdateAllColors();
            }

            // The UV data has just been replaced by the rebuilt character's, so re-derive the visible colour rows
            // and the swatch UI from it. Anything populated during the batch used the previous character's UVs.
            PopulatePartColorRows();
            RefreshVisibleColorRows();
        }

        /// <summary>
        ///     Saves a character (Parts and Colors) out to a file which can be imported by the tool into any project.
        /// </summary>
        private void SaveCharacter()
        {
            try
            {
                SaveCharacter(null);
            }
            catch
            {
                Debug.LogWarning("Failed to save character. Please try again.");
            }
        }

        /// <summary>
        ///     Saves a character (Parts and Colors) out to a file which can be imported by the tool into any project to a given path.
        /// </summary>
        /// <param name="savePath">The path to save the character to.</param>
        private void SaveCharacter(string savePath)
        {
            bool showSuccessMessage = false;

            if (string.IsNullOrEmpty(savePath))
            {
                savePath = ShowCharacterSaveDialog();
                showSuccessMessage = true;
            }

            if (string.IsNullOrEmpty(savePath))
            {
                // EditorUtility.DisplayDialog("Save Cancelled", "No save file selected. Saving cancelled.", "OK");
                return;
            }

            string filename = Path.GetFileNameWithoutExtension(savePath);
            SerializedCharacter savedCharacter = CreateSerializedCharacter(filename);

            Serializer serializer = new Serializer();

            File.WriteAllBytes(savePath, Encoding.ASCII.GetBytes(serializer.Serialize(savedCharacter)));
            if (showSuccessMessage)
            {
                EditorUtility.DisplayDialog("Save Successful", "Character successfully saved to " + Path.GetFileName(savePath), "OK");
            }
        }

        /// <summary>
        ///     Gets the undo proxy object, creating it if required.
        /// </summary>
        /// <returns>The undo proxy for this window.</returns>
        private SidekickUndoProxy GetOrCreateUndoProxy()
        {
            if (_undoProxy == null)
            {
                _undoProxy = ScriptableObject.CreateInstance<SidekickUndoProxy>();
                _undoProxy.hideFlags = HideFlags.HideAndDontSave;
            }

            return _undoProxy;
        }

        /// <summary>
        ///     Records the current (pre-change) tool state onto the editor undo stack. Call at gesture level, BEFORE
        ///     the state mutation. Batch flows (presets, loads, undo application itself) are suppressed here and
        ///     record once at their own gesture site instead; repeat calls within one user input event (a color
        ///     picker drag, the torso to wrap cascade) coalesce into a single undo step.
        /// </summary>
        /// <param name="label">The name shown in the Edit menu for this undo step.</param>
        private void RecordUndo(string label)
        {
            if (_loadingContent || _isApplyingUndoState || _applyingPreset || _loadingCharacter || _processingSpeciesChange)
            {
                return;
            }

            SidekickUndoProxy proxy = GetOrCreateUndoProxy();
            if (string.IsNullOrEmpty(proxy.CharacterSnapshot))
            {
                // No baseline committed yet; there is no meaningful state to restore to.
                return;
            }

            if (Undo.GetCurrentGroup() == _lastRecordedUndoGroup)
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(label);
            Undo.RegisterCompleteObjectUndo(proxy, label);
            _lastRecordedUndoGroup = Undo.GetCurrentGroup();
        }

        /// <summary>
        ///     Writes the current tool state into the undo proxy so the next RecordUndo captures it. Call AFTER the
        ///     state has settled. No-ops mid-batch; the batch's single rebuild commits instead.
        /// </summary>
        private void CommitUndoState()
        {
            if (_isApplyingUndoState || _applyingPreset)
            {
                return;
            }

            if (_currentSpecies == null || _currentColorSet == null || _bodyTypeSlider == null)
            {
                return;
            }

            SerializedCharacter state = CreateSerializedCharacter(_OUTPUT_MODEL_NAME);
            Serializer serializer = new Serializer();
            string snapshot = serializer.Serialize(state);
            SidekickUndoProxy proxy = GetOrCreateUndoProxy();
            proxy.CharacterSnapshot = snapshot;
            _lastAppliedSnapshot = snapshot;

            string presetSelections = serializer.Serialize(_presetDefaultValues);
            proxy.PresetSelections = presetSelections;
            _lastAppliedPresetSelections = presetSelections;
        }

        /// <summary>
        ///     Replaces _presetDefaultValues with the selections stored in the given YAML snapshot.
        /// </summary>
        /// <param name="presetSelections">The serialized row label -> preset name dictionary from the undo proxy.</param>
        /// <returns>
        ///     True if the selections were restored; false (leaving the dictionary untouched) for empty or unreadable
        ///     data, e.g. undo steps recorded before preset selections were tracked.
        /// </returns>
        private bool RestorePresetSelections(string presetSelections)
        {
            if (string.IsNullOrEmpty(presetSelections))
            {
                return false;
            }

            try
            {
                Deserializer deserializer = new Deserializer();
                Dictionary<string, string> restored = deserializer.Deserialize<Dictionary<string, string>>(presetSelections);
                if (restored == null)
                {
                    return false;
                }

                _presetDefaultValues = restored;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Sidekick undo: failed to restore preset selections.\n" + ex);
                return false;
            }
        }

        /// <summary>
        ///     Syncs the preset row dropdowns and their previous/next buttons to the current _presetDefaultValues
        ///     without rebuilding the preset UI. Only valid while the rows' choice lists are unchanged (species and
        ///     filters untouched), which holds for the undo/redo paths that call it. Values are set without
        ///     notifying, mirroring how a full rebuild restores selections.
        /// </summary>
        private void SyncPresetRowsFromDefaults()
        {
            foreach (KeyValuePair<string, PresetRowControls> entry in _presetRowControls)
            {
                string rowLabel = entry.Key;
                PresetRowControls controls = entry.Value;

                if (!_presetDefaultValues.TryGetValue(rowLabel, out string restoredValue) || controls.PopupValues.Count == 0)
                {
                    continue;
                }

                // Fall back to a value verified present in the current choices; "None" itself may be absent, as
                // randomizing intentionally strips it from the row's choice list.
                if (!controls.PopupValues.Contains(restoredValue))
                {
                    restoredValue = controls.IncludeNoneValue && controls.PopupValues.Contains("None")
                        ? "None"
                        : controls.PopupValues[0];
                }

                controls.Dropdown.SetValueWithoutNotify(restoredValue);
                _presetDefaultValues[rowLabel] = restoredValue;
                controls.PreviousButton.SetEnabled(controls.Dropdown.index > 0);
                controls.NextButton.SetEnabled(controls.Dropdown.index < controls.PopupValues.Count - 1);
            }
        }

        /// <summary>
        ///     Applies the undo proxy's snapshot to the tool after the editor performs an undo or redo. Editor-wide
        ///     undo events that did not touch the proxy are ignored via the snapshot comparison.
        /// </summary>
        private void OnUndoRedoPerformed()
        {
            if (_undoProxy == null || _loadingContent || _dbManager == null)
            {
                return;
            }

            string snapshot = _undoProxy.CharacterSnapshot;
            string presetSelections = _undoProxy.PresetSelections;
            bool characterChanged = !string.IsNullOrEmpty(snapshot) && snapshot != _lastAppliedSnapshot;
            bool presetsChanged = !string.IsNullOrEmpty(presetSelections) && presetSelections != _lastAppliedPresetSelections;
            if (!characterChanged && !presetsChanged)
            {
                return;
            }

            _isApplyingUndoState = true;
            try
            {
                // Set first so a failed apply cannot re-trigger on the next unrelated undo event.
                _lastAppliedSnapshot = snapshot;
                _lastAppliedPresetSelections = presetSelections;

                // Restore the preset dropdown record before loading the character: a species-changed load rebuilds
                // the preset UI inside ProcessSpeciesChange, and that rebuild must read the restored values.
                bool presetsRestored = RestorePresetSelections(presetSelections);

                if (characterChanged)
                {
                    Deserializer deserializer = new Deserializer();
                    SerializedCharacter character = deserializer.Deserialize<SerializedCharacter>(snapshot);
                    int previousSpeciesId = _currentSpecies != null ? _currentSpecies.ID : -1;
                    LoadSerializedCharacter(character, _showAllColourProperties, true);

                    // A species-unchanged load skips ProcessSpeciesChange's preset UI rebuild, so the rows' choice
                    // lists are unchanged; just re-sync the dropdown values from the restored selections.
                    if (presetsRestored && character.Species == previousSpeciesId)
                    {
                        SyncPresetRowsFromDefaults();
                    }
                }
                else if (presetsRestored)
                {
                    // Preset-only step: same character state, different dropdown selections.
                    SyncPresetRowsFromDefaults();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Sidekick undo: failed to apply the restored state.\n" + ex);
            }
            finally
            {
                _isApplyingUndoState = false;
            }
        }

        /// <summary>
        ///     Crates a serialized character from the current tool selections.
        /// </summary>
        /// <param name="characterName">The name to store in the serialized character.</param>
        /// <returns>A SerializedCharacter from the selections in the tool.</returns>
        private SerializedCharacter CreateSerializedCharacter(string characterName)
        {
            SerializedCharacter savedCharacter = new SerializedCharacter
            {
                Species = _currentSpecies.ID,
                Name = characterName
            };

            List<SerializedPart> usedParts = new List<SerializedPart>();
            foreach (KeyValuePair<CharacterPartType, SidekickPart> entry in _currentCharacter)
            {
                // TODO: Update the part version to use actual version once the information is available.
                usedParts.Add(new SerializedPart(entry.Value.Name, entry.Key, "1"));
            }

            savedCharacter.Parts = usedParts;
            SerializedColorSet savedSet = new SerializedColorSet();
            savedSet.PopulateFromSidekickColorSet(_currentColorSet, _currentSpecies);
            savedCharacter.ColorSet = savedSet;

            savedCharacter.BlendShapes = new SerializedBlendShapeValues()
            {
                BodyTypeValue = _bodyTypeSlider.value,
                BodySizeValue = _bodySizeSlider.value,
                MuscleValue = _musclesSlider.value
            };

            List<SerializedColorRow> savedColorRows = new List<SerializedColorRow>();
            foreach (SidekickColorRow row in _allColorRows)
            {
                savedColorRows.Add(new SerializedColorRow(row));
            }

            savedCharacter.ColorRows = savedColorRows;

            return savedCharacter;
        }

        /// <summary>
        ///     Loads a character (Parts and Colors) into the tool.
        /// </summary>
        private void LoadCharacter()
        {
            string filePath = EditorUtility.OpenFilePanel("Load Character", "", "sk");
            if (string.IsNullOrEmpty(filePath))
            {
                EditorUtility.DisplayDialog("No File Chosen", "No file was chosen to load.", "OK");
                return;
            }

            LoadCharacterFromPath(filePath);
        }

        /// <summary>
        ///     Loads a character (Parts and Colors) into the tool from a .sk file on disk.
        /// </summary>
        /// <param name="filePath">The path of the .sk file to load.</param>
        private void LoadCharacterFromPath(string filePath)
        {
            RecordUndo("Load Character");
            _loadingCharacter = true;

            _bodyPartsTab.value = true;
            SwitchToTab(TabView.Parts);

            byte[] bytes = File.ReadAllBytes(filePath);
            string data = Encoding.ASCII.GetString(bytes);

            Deserializer deserializer = new Deserializer();
            SerializedCharacter savedCharacter = deserializer.Deserialize<SerializedCharacter>(data);

            ForceCombineOptionsOn();
            LoadSerializedCharacter(savedCharacter, _showAllColourProperties);
            _loadingCharacter = false;
        }

        /// <summary>
        ///     Turns on all the prefab options so a loaded .sk file generates a combined mesh with all blend shapes kept.
        ///     Only the current session state and toggles are updated; the user's saved preferences in EditorPrefs are untouched.
        /// </summary>
        private void ForceCombineOptionsOn()
        {
            _combineMeshes = true;
            _combineBodyBlendShapes = true;
            _combineFacialBlendShapes = true;

            _combineMeshesToggle?.SetValueWithoutNotify(true);
            _combineBodyBlendsToggle?.SetValueWithoutNotify(true);
            _combineFacialBlendsToggle?.SetValueWithoutNotify(true);
        }

        /// <summary>
        ///     Opens the tool window (creating it if needed) and loads the given .sk file into it.
        /// </summary>
        /// <param name="filePath">The path of the .sk file to load.</param>
        public static void OpenAndLoadCharacter(string filePath)
        {
            ModularCharacterWindow window = GetWindow<ModularCharacterWindow>("Sidekick Character Tool");
            window.Show();

            if (window._loadingContent)
            {
                // The window is still building its UI and populating tool data; the load runs at the end of AddAllTabContent.
                _pendingLoadFilePath = filePath;
            }
            else
            {
                _callbackQueue.Enqueue(() => window.LoadCharacterFromPath(filePath));
            }
        }

        /// <summary>
        ///     Loads a character into the tool from a serialized character.
        /// </summary>
        /// <param name="serializedCharacter">The serialized character to load.</param>
        /// <param name="showAllColors">Whether to show all colors or not.</param>
        /// <param name="suppressMissingPartsDialog">
        ///     When true, missing parts are logged as a warning instead of shown in a blocking dialog; used by the
        ///     undo/redo path.
        /// </param>
        private void LoadSerializedCharacter(SerializedCharacter serializedCharacter, bool showAllColors, bool suppressMissingPartsDialog = false)
        {
            // Suppress the per-part rebuilds from the dropdown change events (including the ones queued for after this
            // method returns); SchedulePreviewRegeneration below performs a single rebuild once they have all run.
            _applyingPreset = true;

            SidekickSpecies species = SidekickSpecies.GetByID(_dbManager, serializedCharacter.Species);
            _speciesField.value = species.Name;
            ProcessSpeciesChange(species.Name);

            bool hasErrors = false;
            string errorMessage = "The following parts could not be found in your project:\n";
            foreach (CharacterPartType currentType in Enum.GetValues(typeof(CharacterPartType)))
            {
                PartTypeControls currentField = _partSelectionDictionary[currentType];
                SerializedPart part = serializedCharacter.Parts.FirstOrDefault(p => p.PartType == currentType);
                SidekickPart skPart = SidekickPart.SearchForByName(_dbManager, part?.Name);
                if (skPart != null)
                {
                    UpdateResult result = UpdatePartDropdown(currentField, skPart.Name, errorMessage, hasErrors);
                    hasErrors = result.HasErrors;
                    errorMessage = result.ErrorMessage;
                }
                else
                {
                    // The wrap list has no "None" entry; an empty value is what keeps its dropdown disabled.
                    currentField.SetPartDropdownValue(currentType == CharacterPartType.Wrap ? null : "None");
                }

                // The dropdown change event only fires when the value actually changes (and only updates the character
                // while the parts tab is active), so sync the character state from the final dropdown value directly.
                SyncCurrentCharacterFromDropdown(currentType, currentField);
            }

            if (hasErrors)
            {
                if (suppressMissingPartsDialog)
                {
                    Debug.LogWarning(errorMessage);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Assets Missing",
                        errorMessage,
                        "OK"
                    );
                }
            }


            LoadColorSet(serializedCharacter);

            if (serializedCharacter.BlendShapes != null)
            {
                LoadBlendShapes(serializedCharacter);
            }

            // Validate the loaded wrap against the loaded torso and body type (covers the case where the slider value
            // did not change and so its callback did not run).
            SyncWrapToBodyType();

            _showAllColourProperties = showAllColors;
            UpdateColorTabContent();

            SchedulePreviewRegeneration();
        }

        /// <summary>
        ///     Updates the current character state for the given part type from its dropdown's current value, mirroring the state
        ///     changes of <see cref="PartSelectionChangeEvent" /> without the UI side effects.
        /// </summary>
        /// <param name="type">The part type to update.</param>
        /// <param name="field">The part controls for the given part type.</param>
        private void SyncCurrentCharacterFromDropdown(CharacterPartType type, PartTypeControls field)
        {
            string partName = field.PartDropdown.value;
            if (!string.IsNullOrEmpty(partName)
                && partName != "None"
                && _sidekickRuntime.MappedPartDictionary.ContainsKey(type)
                && _sidekickRuntime.MappedPartDictionary[type].TryGetValue(partName, out SidekickPart selectedPart))
            {
                _currentCharacter[type] = selectedPart;

                GameObject partModel = selectedPart.GetPartModel();
                SkinnedMeshRenderer selectedMesh = partModel == null ? null : partModel.GetComponentInChildren<SkinnedMeshRenderer>();
                if (selectedMesh != null)
                {
                    _partDictionary[type] = selectedMesh;
                }

                if (type == CharacterPartType.Torso)
                {
                    _requiresWrap = selectedPart.UsesWrap;
                }
            }
            else
            {
                _currentCharacter.Remove(type);
                _partDictionary.Remove(type);
            }
        }

        /// <summary>
        ///     Enables and populates the wrap when a wrap-using torso is equipped and the body type is above the threshold,
        ///     otherwise clears it. The character state is written directly, because the wrap dropdown's change event only
        ///     reaches the character while the Parts tab is active (the body type slider lives on the Body tab).
        /// </summary>
        /// <returns>True when the wrap was added to or removed from the character, i.e. the model needs rebuilding.</returns>
        private bool SyncWrapToBodyType()
        {
            if (!_partSelectionDictionary.TryGetValue(CharacterPartType.Wrap, out PartTypeControls wrapSelection))
            {
                return false;
            }

            bool torsoEquipped = _partSelectionDictionary.TryGetValue(CharacterPartType.Torso, out PartTypeControls torsoSelection)
                && !string.IsNullOrEmpty(torsoSelection.PartDropdown.value)
                && torsoSelection.PartDropdown.value != "None";
            bool wrapRequired = torsoEquipped && _requiresWrap && _bodyTypeBlendValue > 0;
            bool hadWrap = _currentCharacter.ContainsKey(CharacterPartType.Wrap);

            if (wrapRequired)
            {
                wrapSelection.PartDropdown.SetEnabled(true);

                string current = wrapSelection.PartDropdown.value;
                if (string.IsNullOrEmpty(current) || current == "None" || !wrapSelection.PartDropdown.choices.Contains(current))
                {
                    // Bring back the wrap the user last had rather than rolling a new one; randomise only when there is none.
                    if (!string.IsNullOrEmpty(_lastSelectedWrap) && wrapSelection.PartDropdown.choices.Contains(_lastSelectedWrap))
                    {
                        wrapSelection.SetPartDropdownValue(_lastSelectedWrap);
                    }
                    else
                    {
                        wrapSelection.RandomisePartDropdownValue();
                    }
                }

                _lastSelectedWrap = wrapSelection.PartDropdown.value;
                SyncCurrentCharacterFromDropdown(CharacterPartType.Wrap, wrapSelection);
            }
            else
            {
                wrapSelection.PartDropdown.SetEnabled(false);
                wrapSelection.SetPartDropdownValue(null);
                _currentCharacter.Remove(CharacterPartType.Wrap);
                _partDictionary.Remove(CharacterPartType.Wrap);
            }

            wrapSelection.UpdateControls();

            return hadWrap != _currentCharacter.ContainsKey(CharacterPartType.Wrap);
        }

        /// <summary>
        ///     Updates a part dropdown to select a new part, if the part is not in the dropdown values, `None` is selected instead.
        /// </summary>
        /// <param name="currentField">The dropdown field to update.</param>
        /// <param name="partName">The new part to select.</param>
        /// <param name="errorMessage">The error message to update if the part is not available.</param>
        /// <param name="hasErrors">The error flag to update if an error is encountered.</param>
        /// <returns>A PartUpdateResult with the results of the update.</returns>
        private UpdateResult UpdatePartDropdown(PartTypeControls currentField, string partName, string errorMessage, bool hasErrors)
        {
            _partLockMap[currentField.PartDropdown] = false;

            if (partName == "None" || _allParts.Any(part => part.Name == partName))
            {
                if (!currentField.PartDropdown.choices.Contains(partName) && PartUtils.IsBaseSpeciesPart(partName))
                {
                    currentField.SetPartDropdownValue(currentField.PartDropdown.choices.Find(n => n.Contains("BASE")) ?? "None");
                    if (currentField.PartDropdown.value == "None")
                    {
                        hasErrors = true;
                        errorMessage += partName + "\n";
                    }
                }
                else
                {
                    currentField.SetPartDropdownValue(partName);
                }
            }
            else if (PartUtils.IsBaseSpeciesPart(partName))
            {
                currentField.SetPartDropdownValue(currentField.PartDropdown.choices.Find(n => n.Contains("BASE")) ?? "None");
                if (currentField.PartDropdown.value == "None")
                {
                    hasErrors = true;
                    errorMessage += partName + "\n";
                }
            }
            else
            {
                currentField.SetPartDropdownValue("None");
                hasErrors = true;
                errorMessage += partName + "\n";
            }

            return new UpdateResult(errorMessage, hasErrors);
        }

        /// <summary>
        ///     Loads the color set for a saved character into memory.
        /// </summary>
        /// <param name="savedCharacter">The character to load the color set for.</param>
        private void LoadColorSet(SerializedCharacter savedCharacter)
        {
            _currentColorSet = savedCharacter.ColorSet.CreateSidekickColorSet(_dbManager);
            _colorSetsDropdown.value = "Custom";

            if (savedCharacter.ColorRows == null || savedCharacter.ColorRows.Count == 0)
            {
                // A snapshot saved before the Colors tab was ever populated carries no rows. Rebuild them
                // from the current color map; leaving them empty would make the next tab open fall back to
                // an in-memory set with no database rows and turn every swatch red.
                PopulateColorRowsFromTextures();
            }
            else
            {
                List<SidekickColorRow> newRows = new List<SidekickColorRow>();
                foreach (SerializedColorRow row in savedCharacter.ColorRows)
                {
                    SidekickColorRow newRow = row.CreateSidekickColorRow(_dbManager, _currentColorSet);

                    // A property that no longer exists in the database has nothing to show or paint.
                    if (newRow.ColorProperty != null && newRows.All(existing => existing.ColorProperty.ID != newRow.ColorProperty.ID))
                    {
                        newRows.Add(newRow);
                    }
                }

                AddMissingColorRows(newRows);
                _allColorRows = newRows;
            }
        }

        /// <summary>
        ///     Adds a row for every color property that has none in the given list, so the list covers every property the
        ///     database knows about. A file saved before a property was added (or by an older version of the tool) would
        ///     otherwise leave that property without a row, and any part using it could never show or paint its color.
        /// </summary>
        /// <param name="rows">The rows to complete.</param>
        private void AddMissingColorRows(List<SidekickColorRow> rows)
        {
            List<SidekickColorRow> currentSetColors = SidekickColorRow.GetAllBySet(_dbManager, _currentColorSet);
            Material defaultMaterial = _materialField?.value as Material;
            Texture2D colorMap = defaultMaterial != null ? defaultMaterial.GetTexture(_COLOR_MAP) as Texture2D : null;

            foreach (SidekickColorProperty property in SidekickColorProperty.GetAll(_dbManager))
            {
                if (rows.Any(row => row.ColorProperty.ID == property.ID))
                {
                    continue;
                }

                SidekickColorRow existingRow = currentSetColors.FirstOrDefault(row => row.ColorProperty.ID == property.ID);
                if (existingRow != null)
                {
                    rows.Add(existingRow);
                    continue;
                }

                Color? currentColor = null;
                if (colorMap != null && colorMap.isReadable)
                {
                    currentColor = colorMap.GetPixel(property.U * 2, property.V * 2);
                }

                rows.Add(
                    new SidekickColorRow
                    {
                        ID = -1,
                        ColorSet = _currentColorSet,
                        ColorProperty = property,
                        NiceColor = currentColor ?? Color.red
                    }
                );
            }
        }

        /// <summary>
        ///     Updates the content of the Color Tab.
        /// </summary>
        private void UpdateColorTabContent()
        {
            PopulatePartColorRows();

            // During a batch operation (e.g. loading a .sk file) defer the colour write to the single
            // rebuild so colours don't apply to the old model as a separate visible stage before the
            // parts swap in. DebouncedRegenerationTick reapplies them after the rebuild.
            if (_applyingPreset)
            {
                _reapplyColorsOnRegen = true;
            }
            else
            {
                UpdateAllVisibleColors();
            }

            RefreshVisibleColorRows();
        }

        /// <summary>
        ///     Loads the blend shapes from a saved character into the tool.
        /// </summary>
        /// <param name="savedCharacter">The character to load the blend shapes for.</param>
        private void LoadBlendShapes(SerializedCharacter savedCharacter)
        {
            _bodyTypeSlider.value = savedCharacter.BlendShapes.BodyTypeValue;
            _bodySizeSlider.value = savedCharacter.BlendShapes.BodySizeValue;
            _musclesSlider.value = savedCharacter.BlendShapes.MuscleValue;
        }

        /// <summary>
        ///     Shows the dialog box for where to save the character to, and also validates the save location and filename.
        /// </summary>
        /// <param name="path">The default path to save to.</param>
        /// <returns>The full file path and filename to save the character to.</returns>
        private string ShowCharacterSaveDialog(string path = "")
        {
            string defaultName = _currentSpecies.Name + "-" + _currentColorSet.Name + ".sk";
            string defaultDirectory = "";
            if (!string.IsNullOrEmpty(path))
            {
                defaultName = Path.GetFileName(path);
                defaultDirectory = Path.GetDirectoryName(path);
            }

            string savePath = EditorUtility.SaveFilePanel(
                "Save New Character",
                defaultDirectory,
                defaultName,
                "sk"
            );

            // if (!string.IsNullOrEmpty(savePath) && File.Exists(savePath))
            // {
            //     int option = EditorUtility.DisplayDialogComplex(
            //         "File Already Exists",
            //         "A file already exists with the same name, are you sure you wish to overwrite it?\nThis cannot be undone.",
            //         "Overwrite",
            //         "Rename",
            //         "Cancel"
            //     );
            //
            //     switch (option)
            //     {
            //         // Overwrite.
            //         case 0:
            //             EditorUtility.DisplayDialog("Overwrite Accepted", "Existing file will be overwritten.", "OK");
            //             break;
            //
            //         // Rename.
            //         case 1:
            //             savePath = ShowCharacterSaveDialog(savePath);
            //             break;
            //
            //         // Cancel.
            //         case 2:
            //         default:
            //             savePath = null;
            //             break;
            //     }
            // }

            return savePath;
        }

        /// <summary>
        ///     Exports a created character as a Humanoid FBX model, along with its textures, material and .sk definition.
        /// </summary>
        private void CreateCharacterPrefab()
        {
            try
            {
                string savePath = SelectPrefabSaveLocation();

                if (string.IsNullOrEmpty(savePath))
                {
                    // EditorUtility.DisplayDialog("Save Cancelled", "No save file selected. Saving cancelled.", "OK");
                    return;
                }

                string baseFilename = Path.GetFileNameWithoutExtension(savePath);
                string directoryBase = Path.GetDirectoryName(savePath) ?? string.Empty;
                string directory = Path.Combine(directoryBase, baseFilename);
                string textureDirectory = Path.Combine(directory, "Textures");
                string materialDirectory = Path.Combine(directory, "Materials");
                string meshDirectory = Path.Combine(directory, "Meshes");

                string fbxPath = Path.Combine(meshDirectory, baseFilename + ".fbx");
                string prefabPath = Path.Combine(directory, baseFilename + ".prefab");

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!Directory.Exists(materialDirectory))
                {
                    Directory.CreateDirectory(materialDirectory);
                }

                if (!Directory.Exists(meshDirectory))
                {
                    Directory.CreateDirectory(meshDirectory);
                }

                string savedCharacterPath = Path.Combine(directory, baseFilename + ".sk");
                SaveCharacter(savedCharacterPath);

                //TODO textures are shared between exports!
                SaveTexturesToDisk(textureDirectory, baseFilename);
                // Ensure textures are written to disk before proceeding. As it seems this happens outside the main Unity loop, so can't be easily checked
                // for.
                int cutoff = 0;
                while (cutoff < 1000000000 && Directory.GetFiles(textureDirectory).Length <= 0)
                {
                    cutoff++;
                }

                AssetDatabase.Refresh();

                GameObject clonedModel = GenerateCharacter(_combineMeshes, false);

                List<SkinnedMeshRenderer> allRenderers = clonedModel.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
                SkinnedMeshRenderer clonedRenderer = allRenderers[0];

                // Blend shape baking/filtering already happened inside CreateCharacter, before any bone movement.
                // now do the bone movements!
                _sidekickRuntime.ProcessRigMovementOnBlendShapeChange(SidekickBlendShapeRigMovement.GetAllForProcessing(_dbManager));
                _sidekickRuntime.ProcessBoneMovement(clonedModel);

                Material newMaterial = CreateNewMaterialAssetFromSource(
                    clonedRenderer.sharedMaterial,
                    textureDirectory,
                    materialDirectory,
                    baseFilename,
                    baseFilename
                );

                foreach (SkinnedMeshRenderer renderer in allRenderers)
                {
                    renderer.sharedMaterial = newMaterial;
                }

                ExportCharacterModel(clonedModel, fbxPath, prefabPath, newMaterial);
                DestroyImmediate(clonedModel);
            }
            catch
            {
                Debug.LogWarning("Failed to create character prefab, please try again.");
            }
        }

        /// <summary>
        ///     Prompts the user to select a path and prefab name within the project.
        /// </summary>
        /// <returns>The path and filename to use to save the prefab to.</returns>
        private string SelectPrefabSaveLocation()
        {
            string defaultName = _currentSpecies.Name + "-" + _currentColorSet.Name + ".fbx";

            string savePath = EditorUtility.SaveFilePanelInProject(
                "Save Character Model",
                defaultName,
                "fbx",
                "Select where to save the FBX"
            );

            return savePath;
        }

        /// <summary>
        ///     Sets the material to use the textures at the given location.
        /// </summary>
        /// <param name="material">The material to set the textures on.</param>
        /// <param name="texturePath">The path to set the textures from.</param>
        /// <param name="textureName">Additional naming for the textures, if applicable.</param>
        /// <returns>The material with the paths set on it.</returns>
        private Material SetTextureLinkOnMaterial(Material material, string texturePath, string textureName = "")
        {
            string filename = _TEXTURE_PREFIX;

            if (!string.IsNullOrEmpty(textureName))
            {
                filename += textureName;
            }

            material.SetTexture(_COLOR_MAP, null);
            LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_COLOR_NAME, _COLOR_MAP);
            // TODO: Uncomment when the shader has the these properties are enabled again
            // material.SetTexture(_METALLIC_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_METALLIC_NAME, _METALLIC_MAP);
            // material.SetTexture(_SMOOTHNESS_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_SMOOTHNESS_NAME, _SMOOTHNESS_MAP);
            // material.SetTexture(_REFLECTION_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_REFLECTION_NAME, _REFLECTION_MAP);
            // material.SetTexture(_EMISSION_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_EMISSION_NAME, _EMISSION_MAP);
            // material.SetTexture(_OPACITY_MAP, null);
            // LoadAndAssignTexture(material, texturePath, filename + _TEXTURE_OPACITY_NAME, _OPACITY_MAP);

            return material;
        }

        /// <summary>
        ///     Loads a texture from disk and assigns it to the material in the given texture ID.
        /// </summary>
        /// <param name="material">The material to assign the texture to.</param>
        /// <param name="texturePath">The path to load the texture from.</param>
        /// <param name="textureName">The name of the texture to load.</param>
        /// <param name="textureID">The texture ID to load the texture into on the material.</param>
        private void LoadAndAssignTexture(Material material, string texturePath, string textureName, int textureID)
        {
            string filePath = Path.Combine(texturePath, textureName);
            while (material.GetTexture(textureID) == null)
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                    textureImporter.filterMode = FilterMode.Point;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                    {
                        maxTextureSize = 32,
                        resizeAlgorithm = TextureResizeAlgorithm.Bilinear,
                        format = TextureImporterFormat.RGB24
                    });
                    EditorUtility.SetDirty(textureImporter);
                    textureImporter.SaveAndReimport();
                }

                material.SetTexture(textureID, (Texture2D) AssetDatabase.LoadAssetAtPath(filePath, typeof(Texture2D)));
            }
        }

        /// <summary>
        ///     Creates a new material to assign to the prefab.
        /// </summary>
        /// <param name="sourceMaterial">The existing material from the base model.</param>
        /// <param name="textureDirectory">The directory to save the textures into.</param>
        /// <param name="materialDirectory">The directory to save the material into.</param>
        /// <param name="baseFilename">The base filename to use for all assets.</param>
        /// <param name="textureName">Additional naming for the textures, if applicable.</param>
        /// <returns>The new material cloned from sourceMaterial saved to the asset database.</returns>
        private Material CreateNewMaterialAssetFromSource(
            Material sourceMaterial,
            string textureDirectory,
            string materialDirectory,
            string baseFilename,
            string textureName = ""
        )
        {
            Material clonedMaterial = new Material(sourceMaterial.shader);
            // NOTE: this is copying the texture slots from oldMaterial, so they need to be null'd afterward in SetTextureLinkOnMaterial()
            clonedMaterial.CopyPropertiesFromMaterial(sourceMaterial);
            clonedMaterial = SetTextureLinkOnMaterial(clonedMaterial, textureDirectory, textureName);
            string materialPath = Path.Combine(materialDirectory, baseFilename + ".mat");
            // If the user has chosen to overwrite the prefab, delete the existing assets to replace them.
            if (File.Exists(materialPath))
            {
                File.Delete(materialPath);
            }

            AssetDatabase.CreateAsset(clonedMaterial, materialPath);
            return clonedMaterial;
        }

        /// <summary>
        ///     Exports the combined character to an FBX (mesh + skeleton) in the Meshes folder, imports it as a Humanoid
        ///     model (giving a configurable Avatar sub-asset with the Rig "Configure..." button), then creates a plain
        ///     prefab that references the imported FBX.
        /// </summary>
        /// <param name="rootGameObject">Root game object of the combined character (mesh + bone hierarchy).</param>
        /// <param name="fbxPath">The project-relative path (ending in .fbx) to save the model to.</param>
        /// <param name="prefabPath">The project-relative path (ending in .prefab) to save the prefab to.</param>
        /// <param name="characterMaterial">The generated material to remap the imported model to.</param>
        private void ExportCharacterModel(
            GameObject rootGameObject,
            string fbxPath,
            string prefabPath,
            Material characterMaterial
        )
        {
#if FBX_EXPORTER_AVAILABLE
            // Export the mesh + skeleton hierarchy to FBX. Components such as the Animator are not exported; the rig is
            // rebuilt by the model importer below.
            string exportedPath = ModelExporter.ExportObject(fbxPath, rootGameObject);
            if (string.IsNullOrEmpty(exportedPath))
            {
                Debug.LogWarning("[Sidekick] FBX export failed; no model was written.");
                return;
            }

            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
            ModelImporter importer = (ModelImporter) AssetImporter.GetAtPath(fbxPath);
            if (importer == null)
            {
                Debug.LogWarning("[Sidekick] Could not load the model importer for the exported FBX.");
                return;
            }

            // Apply the studio-standard FBX import settings (sets animationType = Human, among others).
            ApplyStandardFbxImportSettings(importer);

            // Override the avatar setup so each model gets its own configurable Avatar sub-asset.
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            // Remap the FBX's material slot(s) to the generated material so the FBX asset itself uses it.
            // Read the real slot names from the already-imported model (the assumed name can differ from what the
            // FBX actually wrote), and fall back to the material name if none are found.
            if (characterMaterial != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

                HashSet<string> slotNames = new HashSet<string>();
                GameObject importedModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (importedModel != null)
                {
                    foreach (Renderer renderer in importedModel.GetComponentsInChildren<Renderer>())
                    {
                        foreach (Material mat in renderer.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                slotNames.Add(mat.name);
                            }
                        }
                    }
                }

                if (slotNames.Count == 0)
                {
                    slotNames.Add(characterMaterial.name);
                }

                foreach (string slotName in slotNames)
                {
                    importer.AddRemap(
                        new AssetImporter.SourceAssetIdentifier(typeof(Material), slotName),
                        characterMaterial
                    );
                }
            }

            importer.SaveAndReimport();

            // Build a plain prefab that references the imported FBX's mesh, avatar and material.
            GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (fbxAsset == null)
            {
                Debug.LogWarning("[Sidekick] Could not load the imported FBX to build a prefab.");
                return;
            }

            GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(fbxAsset);
            try
            {
                // Unpack so the result is a standalone prefab (not a Prefab Variant), while its components still
                // reference the FBX's mesh, Humanoid avatar and the remapped material.
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                // The FBX round-trip drops SkinnedMeshRenderer blend-shape weights, so restore the Body-tab values
                // (and any kept facial values) from the source model onto the prefab instance before saving.
                CopyBlendShapeWeights(rootGameObject, instance);

                // Apply the generated material (the asset in the sibling Materials/ folder) to the prefab's renderers
                // so the prefab is guaranteed to use it, independent of the FBX importer material remap.
                if (characterMaterial != null)
                {
                    foreach (SkinnedMeshRenderer smr in instance.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        Material[] mats = smr.sharedMaterials;
                        for (int i = 0; i < mats.Length; i++)
                        {
                            mats[i] = characterMaterial;
                        }
                        smr.sharedMaterials = mats;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            finally
            {
                DestroyImmediate(instance);
            }
#else
            // The FBX Exporter package (com.unity.formats.fbx) is not installed, so the model cannot be exported.
            // SyntyPackageHelper prompts to install it; this guard simply lets the assembly compile until it is present.
            Debug.LogError("[Sidekick] FBX export requires the FBX Exporter package (com.unity.formats.fbx). "
                + "Install it via Synty > Package Helper > Install Packages.");
#endif
        }

        /// <summary>
        ///     Applies the studio-standard FBX import settings (previously captured in FBXImporter.preset) to the given
        ///     model importer. Avatar setup and materials are applied separately by the caller so the per-character
        ///     configurable avatar and generated material take precedence.
        /// </summary>
        /// <param name="importer">The model importer for the exported FBX.</param>
        private static void ApplyStandardFbxImportSettings(ModelImporter importer)
        {
            // Model
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = false;
            importer.importBlendShapes = true;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.preserveHierarchy = false;
            importer.sortHierarchyByName = true;

            // Meshes
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.isReadable = true;
            importer.meshOptimizationFlags = (MeshOptimizationFlags) 0;
            importer.addCollider = false;
            importer.keepQuads = true;
            importer.weldVertices = false;
            importer.indexFormat = ModelImporterIndexFormat.Auto;
            importer.swapUVChannels = false;
            importer.generateSecondaryUV = false;

            // Normals & tangents
            importer.importNormals = ModelImporterNormals.Import;
            importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;
            importer.normalSmoothingAngle = 60f;
            importer.importTangents = ModelImporterTangents.Import;
            importer.importBlendShapeNormals = ModelImporterNormals.None;

            // Rig / skinning
            importer.animationType = ModelImporterAnimationType.Human;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 4;
            importer.minBoneWeight = 0.001f;
            importer.optimizeBones = true;
            importer.optimizeGameObjects = false;

            // Animation
            importer.importConstraints = false;
            importer.importAnimation = false;
            importer.importAnimatedCustomProperties = false;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            importer.resampleCurves = true;
        }

        /// <summary>
        ///     Copies SkinnedMeshRenderer blend-shape weights from source to destination, matched by blend-shape name.
        ///     Used after the FBX round-trip (which drops weight values) so the prefab keeps the Body-tab blend-shape
        ///     settings (and any kept facial values).
        /// </summary>
        /// <param name="source">The model whose current blend-shape weights should be copied.</param>
        /// <param name="destination">The model to apply the blend-shape weights to.</param>
        private static void CopyBlendShapeWeights(GameObject source, GameObject destination)
        {
            Dictionary<string, float> weights = new Dictionary<string, float>();
            foreach (SkinnedMeshRenderer smr in source.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh mesh = smr.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    weights[mesh.GetBlendShapeName(i)] = smr.GetBlendShapeWeight(i);
                }
            }

            foreach (SkinnedMeshRenderer smr in destination.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Mesh mesh = smr.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    if (weights.TryGetValue(mesh.GetBlendShapeName(i), out float weight))
                    {
                        smr.SetBlendShapeWeight(i, weight);
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the "outfit" name from the part name.
        ///     TODO: This will be replaced once parts and outfits have a proper relationship.
        /// </summary>
        /// <param name="partName">The part name to parse the "outfit" name from.</param>
        /// <returns>The "outfit" name.</returns>
        private string GetOutfitNameFromPartName(string partName)
        {
            if (string.IsNullOrEmpty(partName))
            {
                return "None";
            }

            return string.Join('_', partName.Substring(3).Split('_').Take(2));
        }

        /// <summary>
        ///     Reads the parts (UV usage and, where the renderers are named after parts, the dropdown selections) and
        ///     colors of a combined character already present in the scene, so the tool reflects that character when
        ///     opened fresh. Any failure leaves the tool in the same state as if no scene character existed.
        /// </summary>
        private void DetectSceneCharacter()
        {
            if (_newModel == null)
            {
                return;
            }

            try
            {
                List<SkinnedMeshRenderer> renderers = _newModel
                    .GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(renderer => renderer.sharedMesh != null)
                    .ToList();

                if (renderers.Count == 0)
                {
                    return;
                }

                _sidekickRuntime.PopulateUVDictionary(renderers);
                UpdatePartUVData();

                bool partsDetected = DetectScenePartSelections(renderers);

                Texture2D colorMap = renderers
                    .Select(renderer => renderer.sharedMaterial)
                    .Where(material => material != null && material.HasProperty(_COLOR_MAP))
                    .Select(material => material.GetTexture(_COLOR_MAP) as Texture2D)
                    .FirstOrDefault(texture => texture != null);

                PopulateColorRowsFromTextures(colorMap);

                if (partsDetected)
                {
                    // Rebuild once so the tool state and scene model stay consistent (the same pattern as loading a
                    // .sk file); this also resets _applyingPreset set by DetectScenePartSelections.
                    SchedulePreviewRegeneration();
                }
            }
            catch (Exception ex)
            {
                // Leave the suppression flag only if a scheduled regeneration is going to clear it.
                if (!_regenerateQueued)
                {
                    _applyingPreset = false;
                }

                Debug.LogWarning("Unable to read the parts and colors of the existing scene character.\n" + ex);
            }
        }

        /// <summary>
        ///     Sets the part dropdowns (and the current character state) from scene renderers named after parts. A
        ///     model built with combined meshes has a single unnamed renderer, so no parts can be recovered there.
        /// </summary>
        /// <param name="renderers">The renderers of the scene character.</param>
        /// <returns>True if at least one part selection was applied; otherwise false.</returns>
        private bool DetectScenePartSelections(List<SkinnedMeshRenderer> renderers)
        {
            Dictionary<CharacterPartType, string> detectedParts = new Dictionary<CharacterPartType, string>();
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                if (renderer.name.Count(c => c == '_') < 2)
                {
                    continue;
                }

                CharacterPartType type = _sidekickRuntime.ExtractPartType(renderer.name);
                if (!detectedParts.ContainsKey(type))
                {
                    detectedParts[type] = renderer.name;
                }
            }

            if (detectedParts.Count == 0)
            {
                return false;
            }

            // Suppress the per-part rebuilds from the dropdown change events; the caller schedules a single
            // regeneration afterwards, which also resets this flag.
            _applyingPreset = true;

            bool anyPartSet = false;
            foreach (KeyValuePair<CharacterPartType, string> entry in detectedParts)
            {
                if (!_partSelectionDictionary.TryGetValue(entry.Key, out PartTypeControls controls))
                {
                    continue;
                }

                SidekickPart part = SidekickPart.SearchForByName(_dbManager, entry.Value);
                if (part == null)
                {
                    continue;
                }

                UpdatePartDropdown(controls, part.Name, string.Empty, false);
                SyncCurrentCharacterFromDropdown(entry.Key, controls);
                anyPartSet = anyPartSet || _currentCharacter.ContainsKey(entry.Key);
            }

            if (!anyPartSet)
            {
                // Nothing was applied, so no regeneration will run to reset the suppression flag.
                _applyingPreset = false;
            }

            return anyPartSet;
        }

        /// <summary>
        ///     Updates the part UV data.
        /// </summary>
        private void UpdatePartUVData()
        {
            if (_sidekickRuntime.CurrentUVDictionary != null)
            {
                _currentUVDictionary = _sidekickRuntime.CurrentUVDictionary;
            }

            if (_sidekickRuntime.CurrentUVList != null)
            {
                _currentUVList = _sidekickRuntime.CurrentUVList;
            }
        }

        /// <summary>
        ///     The available tab views
        /// </summary>
        private enum TabView
        {
            Preset,
            Parts,
            Body,
            Colors,
            Decals,
            Options
        }

        /// <summary>
        ///     The different types of preset dropdown
        /// </summary>
        private enum PresetDropdownType
        {
            Part,
            Body,
            Color,
            Texture
        }

        /// <summary>
        ///     The live controls for one preset row, letting undo/redo re-sync the row's value and button states
        ///     without rebuilding the preset UI.
        /// </summary>
        private class PresetRowControls
        {
            public PopupField<string> Dropdown;
            public Button PreviousButton;
            public Button NextButton;
            public List<string> PopupValues;
            public bool IncludeNoneValue;
        }

        /// <summary>
        ///     Encapsulates the result from a dropdown update attempt.
        /// </summary>
        private class UpdateResult
        {
            public string ErrorMessage { get; private set; }
            public bool HasErrors { get; private set; }

            public UpdateResult(string errorMessage, bool hasErrors)
            {
                ErrorMessage = errorMessage;
                HasErrors = hasErrors;
            }
        }
    }
}
#endif
