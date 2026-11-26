using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct TimeWindow
    {
        [field: SerializeField, Range(0f, 1f)]
        public float Start { get; private set; }
        
        [field: SerializeField, Range(0f, 1f)]
        public float End { get; private set; }
        
        public bool Contains(float t)
        {
            return t >= Start && t <= End;
        }

        public bool Overlaps(float previousT, float currentT)
        {
            return Start <= currentT && End > previousT;
        }
    }
}