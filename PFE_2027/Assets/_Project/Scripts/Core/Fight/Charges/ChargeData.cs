using PFE.Core.Scripts.Enemy.Attacks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Fight.Charges
{
    // One level of a charge (ChargeUp flag), played by the HitboxRunner while the input is held.
    // Releasing the input while it plays brings out its Release: what you see is what comes out.
    // Expected flags: LoopAnimation, ChargeTime if it has a Next level, MovementLock to stand still.
    [CreateAssetMenu(menuName = "PFE/Actions/Charge", fileName = "Charge")]
    public class ChargeData : HitboxData
    {
        // Action that comes out when the input is released while this level plays
        [field: SerializeField, BoxGroup("Charge")]
        public HitboxData Release { get; private set; }

        // Level played once the ChargeTime of this one elapsed (empty = last level, it plays until the release)
        [field: SerializeField, BoxGroup("Charge")]
        public ChargeData Next { get; private set; }
    }
}
