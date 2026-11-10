namespace PFE.Core.Scripts.ECS
{
    public readonly struct EntityAddress
    {
        public readonly World world;
        public readonly Entity entity;

        public EntityAddress(World world, Entity entity)
        {
            this.world = world;
            this.entity = entity;
        }

        public bool AddComponent<T>(in T component) where T : struct, IEntityComponent 
            => world.AddComponent<T>(entity, component);

        public bool RemoveComponent<T>() where T : struct, IEntityComponent 
            => world.RemoveComponent<T>(entity);

        public bool TryGetComponent<T>(out T component) where T : struct, IEntityComponent 
            => world.TryGetComponent(entity, out component);

        public bool TryGetComponentRef<T>(out ComponentRef<T> component) where T : struct, IEntityComponent 
            => world.TryGetComponentRef(entity, out component);
        
        public void Activate() => world.Activate(entity);
        public void Deactivate() => world.Deactivate(entity);
    }
}