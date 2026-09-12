#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract partial class ElementComponentData : IComponentEditor
    {
        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public string EditorName { get; private set; } = "Element";

        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public Color EditorColor { get; private set; } = new Color(0.36f, 0.66f, 1.00f);
    }
}
#endif
