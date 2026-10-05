using System;
using PFE.Core.Scripts.Enemy.Attacks;
using UnityEngine;

namespace PFE.Core.Scripts.Attitude
{
    // if abstract => same content as the interface = duplicate into interface
    [Serializable]
    public class AttitudeDataTemplate : IAttitudeData
    {
        // General content shared by every attitude

        [field : SerializeField]
        public AttackData[] AttacksDatas {get ; private set;}
    }
}