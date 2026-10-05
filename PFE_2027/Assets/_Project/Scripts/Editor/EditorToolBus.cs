using System;
using System.Collections.Generic;

namespace PFE.Editor._Project.Scripts.Editor
{
    /// <summary>
    /// Static pub/sub bus letting PfeEditorWindow tabs talk to each other
    /// without referencing each other directly. No event is defined yet:
    /// add dedicated event structs once a tab actually needs to publish/listen to one.
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
