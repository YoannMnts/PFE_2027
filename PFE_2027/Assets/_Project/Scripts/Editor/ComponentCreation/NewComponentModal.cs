using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFE.Editor.ComponentCreation
{
    /// <summary>
    /// Fenêtre modale de création d'un nouveau ComponentData : demande un nom et une famille,
    /// puis délègue la génération à ComponentGenerator.
    /// </summary>
    public sealed class NewComponentModal : EditorWindow
    {
        private TextField nameField;
        private DropdownField familyField;
        private Label errorLabel;
        private FamilyInfo[] families;

        public static void Open()
        {
            NewComponentModal window = CreateInstance<NewComponentModal>();
            window.titleContent = new GUIContent("New Component");
            window.minSize = new Vector2(320, 160);
            window.maxSize = new Vector2(320, 160);
            window.ShowModalUtility();
        }

        private void CreateGUI()
        {
            families = ComponentFamilyCatalog.DiscoverFamilies().ToArray();

            VisualElement root = rootVisualElement;
            root.style.paddingLeft = 12;
            root.style.paddingRight = 12;
            root.style.paddingTop = 12;
            root.style.paddingBottom = 12;

            errorLabel = new Label
            {
                style =
                {
                    color = new Color(0.9f, 0.35f, 0.35f),
                    whiteSpace = WhiteSpace.Normal,
                    marginBottom = 6,
                },
            };

            if (families.Length == 0)
            {
                errorLabel.text = "No family detected (no class implements IComponentEditor).";
                root.Add(errorLabel);
                root.Add(new Button(Close) { text = "Close" });
                return;
            }

            nameField = new TextField("Name");
            root.Add(nameField);

            familyField = new DropdownField("Family", families.Select(f => f.EditorName).ToList(), 0);
            root.Add(familyField);

            root.Add(errorLabel);

            VisualElement buttonRow = new() { style = { flexDirection = FlexDirection.Row, marginTop = 8 } };

            Button cancelButton = new(Close) { text = "Cancel" };
            Button createButton = new(TryCreate) { text = "Create" };
            createButton.style.marginLeft = 8;

            buttonRow.Add(cancelButton);
            buttonRow.Add(createButton);
            root.Add(buttonRow);
        }

        private void TryCreate()
        {
            FamilyInfo family = families[Mathf.Max(0, familyField.index)];

            if (!ComponentGenerator.BeginCreate(nameField.value, family, out string error))
            {
                errorLabel.text = error;
                return;
            }

            Close();
        }
    }
}
