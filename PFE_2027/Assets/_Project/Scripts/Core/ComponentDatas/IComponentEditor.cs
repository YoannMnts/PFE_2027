#if UNITY_EDITOR
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    /// <summary>
    /// Implémenté par les classes "famille" de ComponentData (Element, Passive, BasicAttack, SubAttack, ...)
    /// pour permettre à des outils d'éditeur (ex: DatabaseBrowserView) de les regrouper et de les
    /// distinguer visuellement, sans dépendre d'un enum central. Ajouter une nouvelle famille = implémenter
    /// cette interface sur sa classe de base, aucune modification des outils d'éditeur n'est nécessaire.
    /// </summary>
    public interface IComponentEditor
    {
        string EditorName { get; }
        Color EditorColor { get; }
    }
}
#endif
