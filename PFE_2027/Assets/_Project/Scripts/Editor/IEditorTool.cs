using UnityEngine.UIElements;

namespace PFE.Editor
{
    public interface IEditorTool
    {
        string DisplayName { get; }
        string Icon { get; }
        int Order { get; }

        VisualElement BuildUI();
        void OnActivated();
        void OnDeactivated();
    }
}
