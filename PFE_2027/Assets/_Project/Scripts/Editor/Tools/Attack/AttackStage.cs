using System;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Editor._Project.Scripts.Editor.Tools.Animation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    public sealed class AttackStage : PreviewSceneStage
    {
        [SerializeField] private AttackData attack;
        [SerializeField] private GameObject characterPrefab;
        [SerializeField] private GameObject characterInstance;
        [SerializeField] private float currentSeconds;

        [SerializeField] private int selectedHitbox = -1;

        // A single SerializedObject shared by the tab and the scene handles (recreated after a recompile)
        [NonSerialized] private SerializedObject serializedAttack;

        public static event Action<AttackStage> OnOpened;
        public static event Action OnClosed;
        public static event Action<int> OnSelectionChanged;
        
        public static AttackStage Current => StageUtility.GetCurrentStage() as AttackStage;

        public AttackData Attack => attack;
        public GameObject Character => characterInstance;
        public float CurrentSeconds => currentSeconds;
        public SerializedObject SerializedAttack => serializedAttack ??= new SerializedObject(attack);

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
        public float NormalizedTime => attack != null && attack.AnimationClip != null
            ? currentSeconds / attack.AnimationClip.length
            : 0f;

        public static void Open(AttackData attack, GameObject characterPrefab)
        {
            if (attack == null || attack.AnimationClip == null)
            {
                Debug.LogWarning("[AttackStage] The attack has no animation clip.");
                return;
            }

            if (characterPrefab == null)
            {
                Debug.LogWarning($"[AttackStage] No preview character found for '{attack.name}'.");
                return;
            }

            AttackStage stage = CreateInstance<AttackStage>();
            stage.attack = attack;
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

            serializedAttack?.Dispose();
            serializedAttack = null;

            base.OnCloseStage();
        }

        protected override GUIContent CreateHeaderContent()
            => new(attack != null ? $"Attack: {attack.name}" : "Attack",
                EditorGUIUtility.IconContent("AnimationClip Icon").image);

        public void Sample(float seconds)
        {
            AnimationClip clip = attack != null ? attack.AnimationClip : null;
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

        // Finds a preview character automatically: the animation profile whose Animator uses the same controller as the attack
        public static GameObject FindPreviewCharacter(AttackData attack)
        {
            if (attack == null || attack.Controller == null)
                return null;

            foreach (string guid in AssetDatabase.FindAssets("t:AnimationProfile"))
            {
                var profile = AssetDatabase.LoadAssetAtPath<AnimationProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if (profile == null || profile.Character == null)
                    continue;

                Animator animator = profile.Character.GetComponentInChildren<Animator>();
                if (animator != null && animator.runtimeAnimatorController == attack.Controller)
                    return profile.Character;
            }

            return null;
        }
    }
}