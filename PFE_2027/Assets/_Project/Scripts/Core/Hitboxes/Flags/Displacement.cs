using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Moves the owner while its window lasts: Distance meters along where it faces when the window opens.
    // ProgressOverWindow: X range stretched over the window, Y = part of Distance travelled (last key = 100%).
    [Serializable]
    public struct Displacement : IWindowFlag
    {
        [field: SerializeField, Min(0f)]
        public float Distance { get; private set; }

        [field: SerializeField]
        public AnimationCurve ProgressOverWindow { get; private set; }
    }
}
