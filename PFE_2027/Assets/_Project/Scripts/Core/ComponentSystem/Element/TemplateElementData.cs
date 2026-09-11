using PFE.Core.Scripts.GameSettings;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    [CreateAssetMenu(menuName = "PFE/ComponentSystem/Element", fileName = "TemplateElementData")]
    public class TemplateElementData : ElementComponentData
    {
        [SerializeField] private StageStat<int> stacks;

#if UNITY_EDITOR
        protected override void RefreshStageStats() => stacks.EnsureSize();
#endif
    }
}
