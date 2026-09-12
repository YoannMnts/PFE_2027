#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract partial class BasicAttackComponentData : IComponentEditor
    {
        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public string EditorName { get; private set; } = "Basic Attack";

        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public Color EditorColor { get; private set; } = new Color(0.46f, 0.78f, 0.51f);
    }
}
#endif
