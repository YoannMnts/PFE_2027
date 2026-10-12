using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PFE.Gameplay.Scripts.Animations
{
    // Plays action clips (attacks, pushbacks...) on top of the locomotion controller, without any state in the
    // Animator. A PlayableGraph mixes the controller (input 0) with two action slots (inputs 1 and 2): a new
    // action goes in the free slot and crossfades with the previous one. When an action ends, its weight fades
    // out and the locomotion (idle / walk / run) comes back on its own.
    [RequireComponent(typeof(Animator)), DisallowMultipleComponent]
    public sealed class ActionAnimator : MonoBehaviour
    {
        public const float DEFAULT_FADE_IN = 0.05f;
        public const float DEFAULT_FADE_OUT = 0.1f;

        private const int LOCOMOTION_INPUT = 0;
        private const int SLOT_COUNT = 2;
        private const int NO_SLOT = -1;

        private struct Slot
        {
            public AnimationClipPlayable playable;
            public AnimationClip clip;
            public float weight;
            public float targetWeight;
            public float fadeSpeed;         // Weight change per second
            public float fadeOutDuration;
            public int playId;
        }

        private readonly Slot[] slots = new Slot[SLOT_COUNT];

        private Animator animator;
        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private AnimatorControllerPlayable locomotion;
        private int activeSlot = NO_SLOT;
        private int nextPlayId;
        private bool isInitialized;

        public bool IsPlayingAction => activeSlot != NO_SLOT;

        private void Awake() => EnsureInitialized();

        private void OnEnable()
        {
            if (graph.IsValid())
                graph.Play();
        }

        private void OnDisable()
        {
            if (graph.IsValid())
                graph.Stop();
        }

        private void OnDestroy()
        {
            if (graph.IsValid())
                graph.Destroy();
        }

        // Lazy: callers may use it before its Awake ran (Awake order between objects is not guaranteed)
        private void EnsureInitialized()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            animator = GetComponent<Animator>();

            // The graph owns the controller from now on: the Animator must not run it a second time
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            animator.runtimeAnimatorController = null;

            graph = PlayableGraph.Create($"{name} - ActionAnimator");
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            mixer = AnimationMixerPlayable.Create(graph, 1 + SLOT_COUNT);
            if (controller != null)
            {
                locomotion = AnimatorControllerPlayable.Create(graph, controller);
                graph.Connect(locomotion, 0, mixer, LOCOMOTION_INPUT);
            }
            mixer.SetInputWeight(LOCOMOTION_INPUT, 1f);

            AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation", animator);
            output.SetSourcePlayable(mixer);
            graph.Play();
        }

        // ---------------------------------------------------------------- locomotion parameters

        public void SetFloat(int id, float value)
        {
            EnsureInitialized();
            if (locomotion.IsValid())
                locomotion.SetFloat(id, value);
        }

        public void SetBool(int id, bool value)
        {
            EnsureInitialized();
            if (locomotion.IsValid())
                locomotion.SetBool(id, value);
        }

        // ---------------------------------------------------------------- actions

        // Starts the clip in the free slot. Returns the id of this play (or -1 without clip): pass it to
        // TryGetNormalizedTime / Stop to know whether this play is still the current action.
        public int Play(AnimationClip clip, float fadeIn = DEFAULT_FADE_IN, float fadeOut = DEFAULT_FADE_OUT)
        {
            EnsureInitialized();
            if (clip == null)
                return -1;

            int slotIndex = activeSlot == 0 ? 1 : 0;
            int input = slotIndex + 1;
            float fadeSpeed = fadeIn > 0f ? 1f / fadeIn : float.MaxValue;

            ref Slot slot = ref slots[slotIndex];
            if (slot.playable.IsValid())
            {
                graph.Disconnect(mixer, input);
                slot.playable.Destroy();
            }

            slot.playable = AnimationClipPlayable.Create(graph, clip);
            slot.playable.SetTime(0d);
            graph.Connect(slot.playable, 0, mixer, input);
            mixer.SetInputWeight(input, 0f);

            slot.clip = clip;
            slot.weight = 0f;
            slot.targetWeight = 1f;
            slot.fadeSpeed = fadeSpeed;
            slot.fadeOutDuration = fadeOut;
            slot.playId = ++nextPlayId;

            // The previous action fades out while the new one fades in
            if (activeSlot != NO_SLOT)
            {
                ref Slot previous = ref slots[activeSlot];
                previous.targetWeight = 0f;
                previous.fadeSpeed = fadeSpeed;
            }

            activeSlot = slotIndex;
            return slot.playId;
        }

        // Fades the action out if it is still the current one (cancelled by a dash, a knockback...)
        public void Stop(int playId, float fadeOut = DEFAULT_FADE_OUT)
        {
            if (!IsCurrent(playId))
                return;

            ref Slot slot = ref slots[activeSlot];
            slot.targetWeight = 0f;
            slot.fadeSpeed = fadeOut > 0f ? 1f / fadeOut : float.MaxValue;
            activeSlot = NO_SLOT;
        }

        // Normalized time (0 to 1) of the play, false once it ended or was replaced by another action
        public bool TryGetNormalizedTime(int playId, out float normalizedTime)
        {
            if (!IsCurrent(playId))
            {
                normalizedTime = 0f;
                return false;
            }

            ref Slot slot = ref slots[activeSlot];
            normalizedTime = Mathf.Clamp01((float)(slot.playable.GetTime() / slot.clip.length));
            return true;
        }

        private bool IsCurrent(int playId) =>
            playId >= 0 && activeSlot != NO_SLOT
            && slots[activeSlot].playId == playId && slots[activeSlot].playable.IsValid();

        // ---------------------------------------------------------------- weights

        private void Update()
        {
            if (!isInitialized)
                return;

            UpdateActiveAction();
            UpdateWeights(Time.deltaTime);
        }

        private void UpdateActiveAction()
        {
            if (activeSlot == NO_SLOT)
                return;

            ref Slot active = ref slots[activeSlot];
            double time = active.playable.GetTime();
            double length = active.clip.length;

            // The fade out starts before the end, so the locomotion is fully back when the clip finishes
            if (active.targetWeight > 0f && length - time <= active.fadeOutDuration)
            {
                active.targetWeight = 0f;
                active.fadeSpeed = active.fadeOutDuration > 0f ? 1f / active.fadeOutDuration : float.MaxValue;
            }

            if (time >= length)
                activeSlot = NO_SLOT;
        }

        private void UpdateWeights(float deltaTime)
        {
            float actionWeight = 0f;

            for (int i = 0; i < SLOT_COUNT; i++)
            {
                ref Slot slot = ref slots[i];
                if (!slot.playable.IsValid())
                    continue;

                slot.weight = Mathf.MoveTowards(slot.weight, slot.targetWeight, slot.fadeSpeed * deltaTime);
                int input = i + 1;

                // Finished and faded out: the slot is freed (nothing left to evaluate).
                // Never the active slot: its end is decided by the clip time (UpdateActiveAction), not by its weight.
                if (i != activeSlot && slot.weight <= 0f && slot.targetWeight <= 0f)
                {
                    graph.Disconnect(mixer, input);
                    slot.playable.Destroy();
                    slot.clip = null;
                    continue;
                }

                mixer.SetInputWeight(input, slot.weight);
                actionWeight += slot.weight;
            }

            mixer.SetInputWeight(LOCOMOTION_INPUT, Mathf.Clamp01(1f - actionWeight));
        }
    }
}
