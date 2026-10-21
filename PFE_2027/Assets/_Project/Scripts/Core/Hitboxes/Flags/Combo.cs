using System;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [Serializable]
    public struct Combo : IWindowFlag
    {
        [field: SerializeField] 
        public AttackData NextAttack { get; private set; }
    }
}