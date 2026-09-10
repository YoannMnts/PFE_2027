using PFE.Core.Scripts.Attacks;
using PFE.Core.Scripts.Databases;
using UnityEngine;

namespace PFE.Core.Scripts.AIPattern
{
    //si abstract => contenu semblable à l'interface = duplicate into interface
    public abstract class AttitudeData : GameDatabaseObject, IAttitudeData
    {
        //Contenu général et universel de toutes les attitudes

        [field : SerializeField]
        public AttackData[] AttacksData {get ; private set;}
        
        
    }
}