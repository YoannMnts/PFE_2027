using System;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Editor.ComponentCreation;
using PFE.Editor.DatabaseBrowser;

namespace PFE.Editor.Tools
{
    public sealed class ComponentBrowserTool : DatabaseBrowserTool<ComponentData>
    {
        public override string DisplayName => "Composant";
        public override string Icon => "◆";
        public override int Order => 20;

        protected override Action OnCreateRequested => NewComponentModal.Open;
    }
}
