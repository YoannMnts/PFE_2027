using System;
using PFE.Core.Scripts.Fight.Charges;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Read by the owner when the action hits: the owner goes straight into this charge level, without charging,
    // and its Release comes out on the next release of the input (dash chain: a dash that hits makes the next one charged)
    [Serializable]
    public struct ChargeOnHit : IAnimationFlag
    {
        [field: SerializeField]
        public ChargeData Charge { get; private set; }
    }
}
