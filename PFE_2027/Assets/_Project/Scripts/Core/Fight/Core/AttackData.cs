using System;
using System.Collections.Generic;
using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    public abstract class AttackData : HitboxData, IData
    {
        [field : SerializeField, BoxGroup("Metrics")]
        public float AttackRange  { get; private set; } 

        [field: SerializeField, BoxGroup("Combo")]
        public AttackData Next { get; private set; }
    }
}