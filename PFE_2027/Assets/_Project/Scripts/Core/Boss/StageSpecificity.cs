using System;
using PFE.Core.Scripts.AIPattern;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace PFE.Core.Scripts
{
    [Serializable]
    public struct StageSpecificity
    {
        [field : SerializeField, Range(0, 100)]
        public int Health { get; private set; }
        
        //TODO regarder pourquoi on ne peut pas utilser l'interface
        [field : SerializeField]
        public AttitudeData AttitudeData { get; private set; }
        
        [field : SerializeField]
        public Transform ArenaPrefab { get; private set; }
        
        [field : SerializeField]
        public AudioSource ArenaMusic { get; private set; }
    }
}