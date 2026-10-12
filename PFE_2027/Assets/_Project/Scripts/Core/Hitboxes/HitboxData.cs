using System;
using System.Collections.Generic;
using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public abstract class HitboxData : GameDatabaseObject
    {
        [field: SerializeField, BoxGroup("Animation")]
        public RuntimeAnimatorController Controller { get; private set; }

        [field: SerializeField, BoxGroup("Animation"), ValueDropdown("GetStateNames")]
        [field: InfoBox("This state doesn't exist in the controller.", InfoMessageType.Error, "IsStateMissing")]
        public string StateName { get; private set; }

        [field: SerializeField, BoxGroup("Animation"), ReadOnly]
        public AnimationClip AnimationClip { get; private set; }
        
        [SerializeField, BoxGroup("Windows")]
        private HitboxWindow[] hitboxes = Array.Empty<HitboxWindow>();
        
        [SerializeField, BoxGroup("Windows")]
        private AdditionalWindow[] windows = Array.Empty<AdditionalWindow>();
        
        public ReadOnlySpan<HitboxWindow> Hitboxes => hitboxes;

        public ReadOnlySpan<AdditionalWindow> Windows => windows;
        
        public int StateHash { get; private set; }

        private void OnEnable() => StateHash = Animator.StringToHash(StateName ?? string.Empty);

        protected override void OnValidate()
        {
            base.OnValidate();
            StateHash = Animator.StringToHash(StateName ?? string.Empty);
#if UNITY_EDITOR
            SyncClipFromState();
#endif
        }

#if UNITY_EDITOR
        private IEnumerable<UnityEditor.Animations.AnimatorState> EnumerateStates()
        {
            if (Controller is not UnityEditor.Animations.AnimatorController controller || controller.layers.Length == 0)
                yield break;

            foreach (var state in EnumerateStates(controller.layers[0].stateMachine))
                yield return state;
        }

        private static IEnumerable<UnityEditor.Animations.AnimatorState> EnumerateStates(UnityEditor.Animations.AnimatorStateMachine machine)
        {
            foreach (var child in machine.states)
                yield return child.state;

            foreach (var sub in machine.stateMachines)
            foreach (var state in EnumerateStates(sub.stateMachine))
                yield return state;
        }

        private IEnumerable<string> GetStateNames()
        {
            foreach (var state in EnumerateStates())
                yield return state.name;
        }

        private bool IsStateMissing() => Controller != null && !string.IsNullOrEmpty(StateName) && FindState() == null;

        private UnityEditor.Animations.AnimatorState FindState()
        {
            foreach (var state in EnumerateStates())
                if (state.name == StateName)
                    return state;
            return null;
        }

        private void SyncClipFromState()
        {
            var state = FindState();
            AnimationClip = state != null ? state.motion as AnimationClip : null;   // null if the state plays a Blend Tree
        }
        
#endif
    }
}