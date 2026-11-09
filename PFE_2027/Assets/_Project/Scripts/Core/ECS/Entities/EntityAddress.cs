namespace PFE.Core.Scripts.ECS
{
    public readonly struct EntityAddress
    {
        private readonly World world;
        private readonly Entity entity;

        public EntityAddress(World world, Entity entity)
        {
            this.world = world;
            this.entity = entity;
        }

        public void AddComponent<T>() where T : struct, IEntityComponent 
            => world.AddComponent<T>(entity);

        public void RemoveComponent<T>() where T : struct, IEntityComponent 
            => world.RemoveComponent<T>(entity);

        public void TryGetComponent<T>(out T component) where T : struct, IEntityComponent 
            => world.TryGetComponent(entity, out component);

        public void TryGetComponentRef<T>(out ComponentRef<T> component) where T : struct, IEntityComponent 
            => world.TryGetComponentRef(entity, out component);
        
        public void Activate() => world.Activate(entity);
        public void Deactivate() => world.Deactivate(entity);
    }
}