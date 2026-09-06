using System;
using System.Collections.Generic;

namespace PFE.Core.DataMapping
{
    /// <summary>
    /// One strongly-typed store per domain container type. Closed-generic statics
    /// give each TContainer its own dictionary, so dispatch is a single lookup
    /// with no per-call domain resolution. Registration and dispatch hit the same
    /// dictionary in any order — no init-order hazard.
    /// </summary>
    public static class DomainBucket<TContainer> where TContainer : class, IContainer
    {
        private static readonly Dictionary<Type, TContainer> map = new();

        /// <summary>
        /// Store a built container. One per concrete data type; a duplicate throws
        /// at boot. Called by a behaviour's generated <c>BuildAndRegister</c>.
        /// </summary>
        public static void Add(TContainer container)
        {
            if (container is null) throw new ArgumentNullException(nameof(container));
            if (!map.TryAdd(container.DataType, container))
            {
                throw new InvalidOperationException(
                    $"A container is already registered for data type {container.DataType} " +
                    $"in domain {typeof(TContainer).Name}. One behaviour per data type.");
            }
        }

        public static bool TryGet(IData data, out TContainer container)
        {
            if (data is null) { container = null; return false; }
            return map.TryGetValue(data.GetType(), out container);
        }

        /// <summary>Clear registrations (tests / edit-mode domain reload).</summary>
        public static void Clear() => map.Clear();
    }
}
