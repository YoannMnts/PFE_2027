using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace PFE.Core.Scripts.Pilgrims
{
    [CreateAssetMenu(menuName = "PFE/Npc/Pilgrim")]
    public class PilgrimData : NpcData
    {
        [field:  SerializeField, Range(0, 10), BoxGroup("Metrics")]
        public float TargetPositionDistance { get;  private set; }
    }
}