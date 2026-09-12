using System;
using System.Collections.Generic;

namespace PFE.Editor
{
    /// <summary>
    /// Bus pub/sub statique permettant à des onglets de PfeEditorWindow de communiquer
    /// sans se référencer directement entre eux. Aucun event n'est défini pour l'instant :
    /// ajoute des structs d'event dédiées le jour où un onglet a réellement besoin d'en publier/écouter une.
    /// </summary>
    public static class EditorToolBus
    {
        private static readonly Dictionary<Type, Delegate> handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            handlers[typeof(T)] = handlers.TryGetValue(typeof(T), out Delegate existing)
                ? Delegate.Combine(existing, handler)
                : handler;
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (!handlers.TryGetValue(typeof(T), out Delegate existing))
                return;

            Delegate combined = Delegate.Remove(existing, handler);
            if (combined == null)
                handlers.Remove(typeof(T));
            else
                handlers[typeof(T)] = combined;
        }

        public static void Publish<T>(T eventData)
        {
            if (handlers.TryGetValue(typeof(T), out Delegate existing))
                ((Action<T>)existing)?.Invoke(eventData);
        }
    }
}
