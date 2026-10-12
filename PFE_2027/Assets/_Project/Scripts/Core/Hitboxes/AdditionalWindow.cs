using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct AdditionalWindow
    {
        [field: SerializeField]
        public WindowFlags Flags { get; private set; }

        [field: SerializeField]
        public TimeWindow Window { get; private set; }
    }
}
