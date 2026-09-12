using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor.DatabaseBrowser
{
    public abstract class DatabaseBrowserTool<T> : IEditorTool where T : ScriptableObject
    {
        private const string BrowserStylePath = "Assets/_Project/Scripts/Editor/DatabaseBrowser/DatabaseBrowser.uss";

        private DatabaseBrowserView<T> view;

        public abstract string DisplayName { get; }
        public abstract string Icon { get; }
        public virtual int Order => 50;

        /// <summary>
        /// Callback appelé quand l'utilisateur clique sur le bouton de création (masqué si null).
        /// À surcharger dans les outils qui proposent une création guidée (ex: ComponentBrowserTool).
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
