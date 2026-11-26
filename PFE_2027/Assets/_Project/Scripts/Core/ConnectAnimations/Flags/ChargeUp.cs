using System;
using PFE.Core.Scripts.Fight.Charges;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Read by AnimationRunner.TryBeginCharge: the action can be charged, starting with this level.
    // On release, the Release of the level playing comes out (none: the owner plays the action itself).
    [Serializable]
    public struct ChargeUp : IAnimationFlag
    {
        [field: SerializeField]
        public ChargeData Charge { get; private set; }
    }
}
