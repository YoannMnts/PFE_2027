using System;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Receiver of the Animation Events baked in the shared Traversal Pro clips (walk / run / land).
    // Must sit on the same GameObject as the Animator, otherwise Unity logs "has no receiver".
    [RequireComponent(typeof(Animator))]
    public sealed class NpcAnimationEvents : MonoBehaviour
    {
        public event Action OnFootstepped;
        public event Action OnLanded;

        // Called by the "OnFootstep" Animation Event (walk / run clips)
        public void OnFootstep() => OnFootstepped?.Invoke();

        // Called by the "OnLand" Animation Event (landing clips)
        public void OnLand() => OnLanded?.Invoke();
    }
}
