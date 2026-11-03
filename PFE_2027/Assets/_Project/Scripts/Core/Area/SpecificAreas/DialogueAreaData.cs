using PFE.Core.Scripts.Dialogues;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Area
{
    [CreateAssetMenu(menuName = "PFE/Area/Dialogue", fileName = "Dialogue")]
    public class DialogueAreaData : AreaData
    {
        [field: SerializeField, BoxGroup("Base")]
        public TalkData TalkData { get; private set; }
    }
}