using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Read by the HitboxRunner while a ChargeData plays: after Duration seconds, its Next level takes its place
    // (useless on the last level)
    [Serializable]
    public struct ChargeTime : IDataFlag
    {
        [field: SerializeField, Min(0f)]
        public float Duration { get; private set; }
    }
}
