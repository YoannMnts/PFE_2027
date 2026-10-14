using System;
using PFE.Core.Scripts.NPCs;
using PFE.Editor._Project.Scripts.Editor.DatabaseBrowser;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Npcs
{
    // One sub-tab of the Npc tab: the list of one NPC family and how to scaffold a new type of it
    // (data script, behaviour script, data asset). Non-generic base so the tool keeps every family in the same list.
    public abstract class NpcCategory
    {
        public string Title { get; }

        protected NpcCategory(string title) => Title = title;

        // Folders of the generated files (each type gets its own sub-folder for its scripts)
        public abstract string DataScriptFolder { get; }
        public abstract string BehaviourScriptFolder { get; }
        public abstract string AssetFolder { get; }

        // Namespace of the generated data class: used to find its type once the scripts are compiled
        public abstract string DataNamespace { get; }

        public abstract string BuildDataScript(string typeName);
        public abstract string BuildBehaviourScript(string typeName);

        public abstract VisualElement Build(Action onCreateRequested);
        public abstract void Reload();
        public abstract bool Owns(ScriptableObject asset);
        public abstract void Select(ScriptableObject asset);
    }

    public abstract class NpcCategory<T> : NpcCategory where T : NpcData
    {
        private DatabaseBrowserView<T> browser;

        protected NpcCategory(string title) : base(title) { }

        public override VisualElement Build(Action onCreateRequested)
        {
            browser = new DatabaseBrowserView<T>(Title, onCreateRequested);
            return browser.Build();
        }

        public override void Reload() => browser?.Reload();

        public override bool Owns(ScriptableObject asset) => asset is T;

        public override void Select(ScriptableObject asset)
        {
            if (browser == null || asset is not T typed)
                return;

            browser.Reload();
            browser.SetSelection(typed);
        }
    }
}
