using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace PFE.Core.Scripts.Dialogues
{
    //a surement passer en dialogueData
    [CreateAssetMenu(fileName = "TalkData", menuName = "PFE/Dialogue/TalkData")]
    public class TalkData : GameDatabaseObject, IData
    {
        [field: SerializeField, TextArea, BoxGroup("Content")]
        public string[] Texts { get; private set; }
        
        [field: SerializeField, BoxGroup("Content")]
        public ChoiceData[] Choices { get; private set; }
        
        //TODO changer "object" par une compétenceData
        [field: SerializeField, BoxGroup("Content")]
        public object Reward {get ; private set; }
        
    }
}