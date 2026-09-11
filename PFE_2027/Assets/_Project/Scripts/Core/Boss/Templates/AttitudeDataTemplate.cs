using PFE.Core.Scripts.AIPattern;
using UnityEngine;

namespace PFE.Core.Scripts.Templates
{
    [CreateAssetMenu(menuName = "PFE/Boss/AttitudeTemplate")]
    public class AttitudeDataTemplate : AttitudeData
    {
        [field : SerializeField]
        public SphereCollider DamageZone { get; private set; }
        [field : SerializeField]
        public int Cooldown { get; private set; }
    }
}