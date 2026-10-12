using System;
using PFE.Core.Scripts.Enemy.Attacks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Hitbox
{
    // Preview scene used to edit any HitboxData (attack, pushback...) on its animation
    public sealed class HitboxStage : PreviewSceneStage
    {
        [SerializeField] private HitboxData data;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private GameObject characterInstance;
        [SerializeField] private float currentSeconds;

        [SerializeField] private int selectedHitbox = -1;

        // A single SerializedObject shared by the tab and the scene handles (recreated after a recompile)
        [NonSerialized] private SerializedObject serializedData;

        public static event Action<HitboxStage> OnOpened;
        public static event Action OnClosed;
        public static event Action<int> OnSelectionChanged;
        
        public static HitboxStage Current => StageUtility.GetCurrentStage() as HitboxStage;

        public HitboxData Data => data;
        public GameObject Character => characterInstance;
        public float CurrentSeconds => currentSeconds;
        public SerializedObject SerializedData => serializedData ??= new SerializedObject(data);

        // Index of the hitbox edited with the scene handles (-1 = none)
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

        public static void Open(HitboxData data, GameObject characterPrefab)
        {
            if (data == null || data.AnimationClip == null)
            {
                Debug.LogWarning("[HitboxStage] The data has no animation clip.");
                return;
            }

            if (characterPrefab == null)
            {
                Debug.LogWarning($"[HitboxStage] No preview character found for '{data.name}'.");
                return;
            }

            HitboxStage stage = CreateInstance<HitboxStage>();
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
            => new(data != null ? $"Hitbox: {data.name}" : "Hitbox",
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

            SceneView.RepaintAll();
        }
    }
}
