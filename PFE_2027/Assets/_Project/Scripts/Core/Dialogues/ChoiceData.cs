using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Dialogues
{
    public class ChoiceData : GameDatabaseObject, IData
    {
        [field: SerializeField, BoxGroup("Content")]
        public TalkData NextTalk { get; private set; }
    }
}