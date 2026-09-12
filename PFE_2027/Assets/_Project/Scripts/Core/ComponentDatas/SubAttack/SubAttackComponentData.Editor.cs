#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract partial class SubAttackComponentData : IComponentEditor
    {
        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public string EditorName { get; private set; } = "Sub Attack";

        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public Color EditorColor { get; private set; } = new Color(0.91f, 0.51f, 0.77f);
    }
}
#endif
