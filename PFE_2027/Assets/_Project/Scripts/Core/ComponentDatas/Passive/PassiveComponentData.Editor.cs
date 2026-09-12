#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract partial class PassiveComponentData : IComponentEditor
    {
        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public string EditorName { get; private set; } = "Passive";

        [ShowInInspector, ReadOnly, BoxGroup("Editor")]
        public Color EditorColor { get; private set; } = new Color(0.71f, 0.56f, 0.94f);
    }
}
#endif
