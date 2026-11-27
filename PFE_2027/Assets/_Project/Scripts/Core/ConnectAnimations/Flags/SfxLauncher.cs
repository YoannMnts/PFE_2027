using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public struct SfxLauncher : ITimedFlag
    {
        [field: SerializeField]
        public TimeWindow Window { get; private set; }
        
        [field: SerializeField]
        public AudioClip Clip { get; private set; }
    }
}