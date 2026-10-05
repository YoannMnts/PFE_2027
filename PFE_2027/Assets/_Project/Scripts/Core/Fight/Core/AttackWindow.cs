using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct AttackWindow
    {
        [field: SerializeField]
        public AttackFlags Flags { get; private set; }

        [field: SerializeField]
        public TimeWindow Window { get; private set; }
    }
}
