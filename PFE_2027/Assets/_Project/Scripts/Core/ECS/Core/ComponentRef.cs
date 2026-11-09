namespace PFE.Core.Scripts.ECS
{
    public readonly struct ComponentRef<T> where T : IEntityComponent
    {
        public readonly Entity entity;
        public readonly ComponentStore<T> store;
        public readonly World world;
        
        public ComponentRef(Entity entity, ComponentStore<T> store, World world)
        {
            this.entity = entity;
            this.store = store;
            this.world = world;
        }

        public ref T GetValue() => ref store.GetRef(entity.id);
        
        public static implicit operator T(ComponentRef<T> component) => component.GetValue();
    }
}