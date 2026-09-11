using System;
using PFE.Core.Scripts.Attacks;
using PFE.Core.Scripts.Databases;
using UnityEngine;

namespace PFE.Core.Scripts.AIPattern
{
    //si abstract => contenu semblable à l'interface = duplicate into interface
    [Serializable]
    public class AttitudeDataTemplate : IAttitudeData
    {
        //Contenu général et universel de toutes les attitudes

        [field : SerializeField]
        public AttackData[] AttacksDatas {get ; private set;}
    }
}