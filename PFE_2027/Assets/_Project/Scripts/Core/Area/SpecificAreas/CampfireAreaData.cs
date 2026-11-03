using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace PFE.Core.Scripts.Area
{
    [CreateAssetMenu(menuName = "PFE/Area/Campfire", fileName = "Campfire")]
    public class CampfireAreaData : AreaData
    {
        [field: SerializeField, BoxGroup("References", true, false, 2f)]
        public DialogueAreaData DialogueAreaData { get; private set; }
    }
}