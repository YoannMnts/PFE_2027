using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Area
{
    [CreateAssetMenu(menuName = "PFE/Area/Campfire", fileName = "Campfire")]
    public class CampfireAreaData : AreaData
    {
        [field: SerializeField, BoxGroup("References", true, false, 2f)]
        public DialogueAreaData DialogueAreaPrefab { get; private set; }
    }
}