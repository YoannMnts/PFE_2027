using System;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    // Read by the HitboxRunner when the action begins: the clip starts again from the beginning each time it
    // finishes, the action only ends when something stops it (e.g. the release of a charge)
    [Serializable]
    public struct LoopAnimation : IDataFlag
    {
    }
}
