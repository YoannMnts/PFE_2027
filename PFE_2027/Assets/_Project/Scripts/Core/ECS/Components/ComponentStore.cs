using System;

namespace PFE.Core.Scripts.ECS
{
    public sealed class ComponentStore<TComponent> : SparseSet<TComponent>, IComponentStore 
        where TComponent : IEntityComponent
    {
        public ComponentStore(int capacity) : base(capacity)
        {
        }
    }

    public interface IComponentStore
    {
        int Count { get; }
        bool TryRemove(int entityId);
        bool Has(int entityId);
    }
}