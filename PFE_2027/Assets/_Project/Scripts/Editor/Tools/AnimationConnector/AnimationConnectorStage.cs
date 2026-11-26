using System;
using PFE.Core.Scripts.Enemy.Attacks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.AnimationConnector
{
    // Preview scene used to edit any AnimationData (attack, pushback...) on its animation
    public sealed class AnimationConnectorStage : PreviewSceneStage
    {
        [SerializeField] private AnimationData data;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private GameObject characterInstance;
        [SerializeField] private float currentSeconds;

        [SerializeField] private int selectedHitbox = -1;

        // A single SerializedObject shared by the tab and the scene handles (recreated after a recompile)
        [NonSerialized] private SerializedObject serializedData;

        public static event Action<AnimationConnectorStage> OnOpened;
        public static event Action OnClosed;
        public static event Action<int> OnSelectionChanged;
        
        public static AnimationConnectorStage Current => StageUtility.GetCurrentStage() as AnimationConnectorStage;

        public AnimationData Data => data;
        public GameObject Character => characterInstance;
        public float CurrentSeconds => currentSeconds;
        public SerializedObject SerializedData => serializedData ??= new SerializedObject(data);

        // Index in Flags of the Hitbox flag edited with the scene handles (-1 = none)
        public int SelectedHitbox
        {
            get => selectedHitbox;
            set
            {
                if (selectedHitbox == value)
                    return;

                selectedHitbox = value;
                OnSelectionChanged?.Invoke(value);
                SceneView.RepaintAll();
            }
        }
        public float NormalizedTime => data != null && data.AnimationClip != null
            ? currentSeconds / data.AnimationClip.length
            : 0f;

        public static void Open(AnimationData data, GameObject characterPrefab)
        {
            if (data == null || data.AnimationClip == null)
            {
                Debug.LogWarning("[AnimationConnectorStage] The data has no animation clip.");
                return;
            }

            if (characterPrefab == null)
            {
                Debug.LogWarning($"[AnimationConnectorStage] No preview character found for '{data.name}'.");
                return;
            }

            AnimationConnectorStage stage = CreateInstance<AnimationConnectorStage>();
            stage.data = data;
            stage.characterPrefab = characterPrefab;
            StageUtility.GoToStage(stage, true);
        }

        protected override bool OnOpenStage()
        {
            if (!base.OnOpenStage())
                return false;

            characterInstance = (GameObject)PrefabUtility.InstantiatePrefab(characterPrefab, scene);
            characterInstance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            AnimationMode.StartAnimationMode();
            Sample(0f);
            OnOpened?.Invoke(this);
            return true;
        }

        protected override void OnCloseStage()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();

            OnClosed?.Invoke();

            // Global editor setting: give Unity its transform handles back
            UnityEditor.Tools.hidden = false;

            serializedData?.Dispose();
            serializedData = null;

            base.OnCloseStage();
        }

        protected override GUIContent CreateHeaderContent()
            => new(data != null ? $"Animation: {data.name}" : "Animation",
                EditorGUIUtility.IconContent("AnimationClip Icon").image);

        public void Sample(float seconds)
        {
            AnimationClip clip = data != null ? data.AnimationClip : null;
            if (clip == null || characterInstance == null)
                return;

            if (!AnimationMode.InAnimationMode())
                AnimationMode.StartAnimationMode();

            currentSeconds = Mathf.Clamp(seconds, 0f, clip.length);

            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(characterInstance, clip, currentSeconds);
            AnimationMode.EndSampling();

            // Like in game, the character slides along its Displacement windows (preview: along +Z)
            characterInstance.transform.position = Vector3.forward * DisplacementPreview.GetDistanceAt(data, NormalizedTime);

            SceneView.RepaintAll();
        }
    }
}
