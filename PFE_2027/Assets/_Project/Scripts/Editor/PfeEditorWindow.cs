using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor
{
    public sealed class PfeEditorWindow : EditorWindow
    {
        private const string ActiveToolSessionKey = "PFE.Editor.ActiveTool";
        private const string ThemeStylePath = "Assets/_Project/Scripts/Editor/PfeEditorTheme.uss";
        private const string WindowStylePath = "Assets/_Project/Scripts/Editor/PfeEditorWindow.uss";

        private static readonly Color[] IconTints =
        {
            new(0.36f, 0.66f, 1.00f),
            new(0.46f, 0.78f, 0.51f),
            new(0.71f, 0.56f, 0.94f),
            new(0.91f, 0.51f, 0.77f),
            new(0.95f, 0.70f, 0.30f),
            new(0.38f, 0.81f, 0.81f),
        };

        private readonly List<IEditorTool> tools = new();
        private readonly Dictionary<IEditorTool, VisualElement> builtContent = new();
        private VisualElement rail;
        private VisualElement contentContainer;
        private IEditorTool activeTool;

        [MenuItem("PFE/PFE Editor")]
        public static void Open()
        {
            PfeEditorWindow window = GetWindow<PfeEditorWindow>();
            window.titleContent = new GUIContent("PFE Editor");
            window.minSize = new Vector2(720, 420);
            window.Show();
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Row;
            StyleSheetLoader.Load(root, ThemeStylePath);
            StyleSheetLoader.Load(root, WindowStylePath);

            rail = new VisualElement();
            rail.AddToClassList("pe-rail");

            contentContainer = new VisualElement();
            contentContainer.AddToClassList("pe-content");

            root.Add(rail);
            root.Add(contentContainer);

            DiscoverTools();
            BuildRail();

            if (tools.Count == 0)
            {
                contentContainer.Add(new Label("Aucun outil trouvé. Implémente IEditorTool pour en ajouter un."));
                return;
            }

            Activate(ResolveInitialTool());
        }

        private void DiscoverTools()
        {
            tools.Clear();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }
                catch { continue; }

                foreach (Type type in types)
                {
                    if (type == null || type.IsAbstract || type.IsInterface)
                        continue;
                    if (!typeof(IEditorTool).IsAssignableFrom(type))
                        continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    tools.Add((IEditorTool)Activator.CreateInstance(type));
                }
            }

            tools.Sort((a, b) =>
            {
                int cmp = a.Order.CompareTo(b.Order);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.DisplayName, b.DisplayName);
            });
        }

        private void BuildRail()
        {
            rail.Clear();

            for (int i = 0; i < tools.Count; i++)
            {
                IEditorTool tool = tools[i];
                Color tint = IconTints[i % IconTints.Length];

                VisualElement button = new();
                button.AddToClassList("pe-rail__btn");

                Label icon = new(tool.Icon);
                icon.AddToClassList("pe-rail__icon");
                icon.style.backgroundColor = tint;

                Label label = new(tool.DisplayName);
                label.AddToClassList("pe-rail__label");

                button.Add(icon);
                button.Add(label);
                button.RegisterCallback<ClickEvent>(_ => Activate(tool));

                rail.Add(button);
            }
        }

        private IEditorTool ResolveInitialTool()
        {
            string savedName = SessionState.GetString(ActiveToolSessionKey, string.Empty);
            return tools.Find(t => t.DisplayName == savedName) ?? tools[0];
        }

        private void Activate(IEditorTool tool)
        {
            activeTool?.OnDeactivated();
            activeTool = tool;
            SessionState.SetString(ActiveToolSessionKey, tool.DisplayName);

            int activeIndex = tools.IndexOf(tool);
            for (int i = 0; i < rail.childCount; i++)
                rail[i].EnableInClassList("pe-rail__btn--active", i == activeIndex);

            if (!builtContent.TryGetValue(tool, out VisualElement content))
            {
                content = tool.BuildUI();
                builtContent[tool] = content;
            }

            contentContainer.Clear();

            VisualElement header = new();
            header.AddToClassList("pe-header");

            Label headerIcon = new(tool.Icon);
            headerIcon.AddToClassList("pe-header__icon");
            Label headerTitle = new(tool.DisplayName);
            headerTitle.AddToClassList("pe-header__title");

            header.Add(headerIcon);
            header.Add(headerTitle);

            contentContainer.Add(header);
            contentContainer.Add(content);

            tool.OnActivated();
        }
    }
}
