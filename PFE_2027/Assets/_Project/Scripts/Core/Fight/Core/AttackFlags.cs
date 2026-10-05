using System;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // What an attack window does while the animation time is inside it.
    // Serialized as an int: every value is explicit. A new flag takes the next free bit,
    // an existing value never changes and a removed bit is never reused.
    [Flags]
    public enum AttackFlags
    {
        None = 0,
        MovementLock = 1 << 0,
        Combo = 1 << 1,         // The next attack input is accepted
    }
}
