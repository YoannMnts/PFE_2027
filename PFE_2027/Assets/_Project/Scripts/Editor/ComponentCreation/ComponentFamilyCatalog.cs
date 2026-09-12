using System;
using System.Collections.Generic;
using PFE.Core.Scripts.ComponentSystem;
using UnityEditor;
using UnityEngine;

namespace PFE.Editor.ComponentCreation
{
    /// <summary>
    /// Une famille de ComponentData découverte par réflexion (classe abstraite implémentant IComponentEditor).
    /// </summary>
    public readonly struct FamilyInfo
    {
        public readonly Type FamilyType;
        public readonly string EditorName;
        public readonly Color EditorColor;
        public readonly string FolderName;

        public FamilyInfo(Type familyType, string editorName, Color editorColor)
        {
            FamilyType = familyType;
            EditorName = editorName;
            EditorColor = editorColor;
            FolderName = familyType.Name.EndsWith("ComponentData")
                ? familyType.Name.Substring(0, familyType.Name.Length - "ComponentData".Length)
                : familyType.Name;
        }
    }

    /// <summary>
    /// Découvre par réflexion les familles de ComponentData disponibles, sans aucune liste en dur :
    /// une nouvelle famille (nouvelle classe abstraite implémentant IComponentEditor) apparaît
    /// automatiquement dès qu'elle est compilée.
    /// </summary>
    public static class ComponentFamilyCatalog
    {
        public static List<FamilyInfo> DiscoverFamilies()
        {
            List<FamilyInfo> result = new();
            HashSet<Type> seenFamilies = new();

            foreach (Type concreteType in TypeCache.GetTypesDerivedFrom<ComponentData>())
            {
                if (concreteType.IsAbstract)
                    continue;

                Type familyType = FindFamilyType(concreteType);
                if (familyType == null || !seenFamilies.Add(familyType))
                    continue;

                ScriptableObject scratch = ScriptableObject.CreateInstance(concreteType);
                try
                {
                    if (scratch is IComponentEditor editor)
                        result.Add(new FamilyInfo(familyType, editor.EditorName, editor.EditorColor));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(scratch);
                }
            }

            result.Sort((a, b) => string.CompareOrdinal(a.EditorName, b.EditorName));
            return result;
        }

        private static Type FindFamilyType(Type concreteType)
        {
            for (Type t = concreteType.BaseType; t != null && t != typeof(ComponentData); t = t.BaseType)
            {
                if (t.IsAbstract && typeof(IComponentEditor).IsAssignableFrom(t))
                    return t;
            }

            return null;
        }
    }
}
