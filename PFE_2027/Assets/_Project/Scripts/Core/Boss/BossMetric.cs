using System;
using PFE.Core.Scripts.AIPattern;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PFE.Core.Scripts
{
    [Serializable]
    public struct BossMetric
    {
        [field : SerializeField, Range(0, 100), BoxGroup("Parameters")]
        public int Health { get; private set; }
        
        [field : SerializeReference, HideLabel, BoxGroup("Parameters")]
        public IAttitudeData AttitudeData { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public Transform ArenaPrefab { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public AudioSource ArenaMusic { get; private set; }
    }
}