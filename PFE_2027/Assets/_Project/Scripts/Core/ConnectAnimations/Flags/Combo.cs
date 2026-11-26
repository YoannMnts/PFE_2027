using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // While its window is open, an attack request chains into NextAttack
    [Serializable]
    public struct Combo : ITimedFlag
    {
        [field: SerializeField]
        public TimeWindow Window { get; private set; }

        [field: SerializeField] 
        public AttackData NextAttack { get; private set; }
    }
}
