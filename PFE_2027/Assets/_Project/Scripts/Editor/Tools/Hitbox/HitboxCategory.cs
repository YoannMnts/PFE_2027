using System;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Hitbox
{
    // One sub-tab of the Hitbox tab: the list of the assets of one HitboxData type, with an "Edit" button.
    // Non-generic base so the tool can keep every category in the same list.
    public abstract class HitboxCategory
    {
        public string Title { get; }

        protected HitboxCategory(string title) => Title = title;

        public abstract VisualElement Build(Action<HitboxData, VisualElement> buildSelectionActions, string searchRoot);
        public abstract void Reload();
    }

    public sealed class HitboxCategory<T> : HitboxCategory where T : HitboxData
    {
        private DatabaseBrowserView<T> browser;

        public HitboxCategory(string title) : base(title) { }

        public override VisualElement Build(Action<HitboxData, VisualElement> buildSelectionActions, string searchRoot)
        {
            // Action is contravariant: an Action<HitboxData, ...> is a valid Action<T, ...>
            browser = new DatabaseBrowserView<T>(Title, buildSelectionActions: buildSelectionActions, searchRoot: searchRoot);
            return browser.Build();
        }

        public override void Reload() => browser?.Reload();
    }
}
