using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Dialogues
{
    public class TalkData : GameDatabaseObject, IData
    {
        //TODO changer "object" par une compétenceData
        [field: SerializeField, BoxGroup("Content")]
        public object Reward {get ; private set; }
        
        [field: SerializeField, BoxGroup("Content")]
        public ChoiceData[] Choice { get; private set; }
        
    }
}