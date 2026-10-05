using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.DatabaseBrowser
{
    public abstract class DatabaseBrowserTool<T> : IEditorTool where T : ScriptableObject
    {
        private const string BrowserStylePath = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";

        private DatabaseBrowserView<T> view;

        public abstract string DisplayName { get; }
        public abstract string Icon { get; }
        public virtual int Order => 50;

        /// <summary>
        /// Callback invoked when the user clicks the create button (hidden if null).
        /// Override in tools that offer a guided creation (e.g. ComponentBrowserTool).
        /// </summary>
        protected virtual Action OnCreateRequested => null;

        public VisualElement BuildUI()
        {
            view = new DatabaseBrowserView<T>(DisplayName, OnCreateRequested);
            VisualElement content = view.Build();
            StyleSheetLoader.Load(content, BrowserStylePath);
            return content;
        }

        public void OnActivated() => view?.Reload();
        public void OnDeactivated() { }
    }
}
