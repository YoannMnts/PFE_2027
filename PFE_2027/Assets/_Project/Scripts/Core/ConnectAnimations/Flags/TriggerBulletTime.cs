using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public class TriggerBulletTime : ITimedFlag
    {
        [field: SerializeField]
        public TimeWindow Window { get; private set; }
    }
}
