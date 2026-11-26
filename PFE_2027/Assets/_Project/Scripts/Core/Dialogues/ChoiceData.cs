using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Dialogues
{
    [CreateAssetMenu(menuName = "PFE/Dialogues/Choice", fileName = "Choice")]
    public class ChoiceData : GameDatabaseObject, IData
    {
        [field: SerializeField, TextArea]
        public string Text { get; private set; }
        
        [field: SerializeField, BoxGroup("Content")]
        public TalkData NextTalk { get; private set; }
    }
}