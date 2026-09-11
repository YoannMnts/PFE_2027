using System;
using PFE.Core.Scripts.AIPattern;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PFE.Core.Scripts
{
    [Serializable]
    public struct BossSpecificity
    {
        [field : SerializeField, Range(0, 100), BoxGroup("Parameters")]
        public int Health { get; private set; }
        
        //TODO regarder pourquoi on ne peut pas utilser l'interface
        [field : SerializeReference, HideLabel, BoxGroup("Parameters")]
        public IAttitudeData AttitudeData { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public Transform ArenaPrefab { get; private set; }
        
        [field : SerializeField, BoxGroup("References")]
        public AudioSource ArenaMusic { get; private set; }
    }
}