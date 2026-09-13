using System;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    /// <summary>
    /// Handle to a component instance registered in a <see cref="ComponentGroup"/>.
    /// Do not construct this yourself — it has no meaning unless it came from
    /// <see cref="TryAddComponent"/>, the only method that registers a matching
    /// entry in <see cref="instances"/>. A hand-made id just won't match anything
    /// and every lookup against it will fail (safely).
    /// </summary>
    public readonly struct ComponentInstanceId : IEquatable<ComponentInstanceId>
    {
        public readonly int value;

        /// <summary>
        /// Do not call directly — use <see cref="ComponentGroup.TryAddComponent"/> instead.
        /// This constructor only stays public because C# has no way to restrict it to a
        /// single method of the containing type; using it elsewhere produces an id that
        /// matches no registered instance.
        /// </summary>
        public ComponentInstanceId(int value) => this.value = value;

        public bool Equals(ComponentInstanceId other) => value == other.value;
        public override bool Equals(object obj) => obj is ComponentInstanceId other && Equals(other);
        public override int GetHashCode() => value;
    }
}