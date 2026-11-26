using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // No movement while its window is open (0 to 1 = the whole action)
    [Serializable]
    public struct MovementLock : ITimedFlag
    {
        [field: SerializeField]
        public TimeWindow Window { get; private set; }
    }
}
