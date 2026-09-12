using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PFE.Core.Scripts.ComponentSystem;
using UnityEditor;
using UnityEngine;

namespace PFE.Editor.ComponentCreation
{
    /// <summary>
    /// Une famille de ComponentData découverte par réflexion (classe abstraite implémentant IComponentEditor).
    /// ComponentInterfaceType/ActionMethodName décrivent l'interface de gameplay correspondante
    /// (ex: IElementComponent&lt;TData&gt;) et sa méthode propre à implémenter (ex: ApplyElement),
    /// retrouvées elles aussi par réflexion — null si la famille n'a pas encore d'interface de ce genre.
    /// </summary>
    public readonly struct FamilyInfo
    {
        public readonly Type FamilyType;
        public readonly string EditorName;
        public readonly Color EditorColor;
        public readonly string FolderName;
        public readonly Type ComponentInterfaceType;
        public readonly string ActionMethodName;

        public FamilyInfo(Type familyType, string editorName, Color editorColor)
        {
            FamilyType = familyType;
            EditorName = editorName;
            EditorColor = editorColor;
            FolderName = familyType.Name.EndsWith("ComponentData")
                ? familyType.Name.Substring(0, familyType.Name.Length - "ComponentData".Length)
                : familyType.Name;

            ComponentInterfaceType = ComponentFamilyCatalog.FindComponentInterface(familyType);
            ActionMethodName = ComponentInterfaceType != null
                ? ComponentFamilyCatalog.FindActionMethodName(ComponentInterfaceType)
                : null;
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

        /// <summary>
        /// Cherche, dans toutes les assemblies, une interface générique à un seul paramètre dont le nom
        /// finit par "Component" et dont la contrainte générique correspond au type de famille donné
        /// (ex: trouve IElementComponent&lt;TData&gt; pour ElementComponentData). Aucune liste en dur :
        /// une nouvelle famille avec son interface de gameplay est retrouvée automatiquement.
        /// </summary>
        public static Type FindComponentInterface(Type familyDataType)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types; }
                catch { continue; }

                foreach (Type type in types)
                {
                    if (type == null || !type.IsInterface || !type.IsGenericTypeDefinition)
                        continue;
                    if (!type.Name.EndsWith("Component`1"))
                        continue;

                    Type[] genericArgs = type.GetGenericArguments();
                    if (genericArgs.Length != 1)
                        continue;

                    if (genericArgs[0].GetGenericParameterConstraints().Contains(familyDataType))
                        return type;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrouve la méthode propre à une interface de famille (ex: ApplyElement) : la seule méthode
        /// publique déclarée directement dessus — ça exclut CanTrigger/Trigger hérités de IComponent&lt;TData&gt;
        /// ainsi que l'implémentation par défaut (privée) de Trigger fournie par l'interface de famille.
        /// </summary>
        public static string FindActionMethodName(Type componentInterfaceType)
        {
            return componentInterfaceType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Select(m => m.Name)
                .FirstOrDefault();
        }
    }
}
