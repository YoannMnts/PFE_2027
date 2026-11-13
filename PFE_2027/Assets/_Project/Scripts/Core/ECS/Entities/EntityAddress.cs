namespace PFE.Core.Scripts.ECS
{
    public readonly struct EntityAddress
    {
        public readonly World world;
        public readonly Entity entity;

        public bool IsValid => world != null;
        public EntityAddress(World world, Entity entity)
        {
            this.world = world;
            this.entity = entity;
        }

        public bool AddComponent<T>(in T component) where T : struct, IEntityComponent 
            => IsValid && world.AddComponent<T>(entity, component);

        public bool RemoveComponent<T>() where T : struct, IEntityComponent 
            => IsValid && world.RemoveComponent<T>(entity);

        public bool TryGetComponent<T>(out T component) where T : struct, IEntityComponent
        {
            if (!IsValid)
            {
                component = default;
                return false;
            }

            return world.TryGetComponent(entity, out component);
        }

        public bool TryGetComponentRef<T>(out ComponentRef<T> component) where T : struct, IEntityComponent
        {
            if (!IsValid)
            {
                component = default;
                return false;
            }
            
            return world.TryGetComponentRef(entity, out component);
        }

        public void Activate()
        {
            if (!IsValid)
                return;
            
            world.Activate(entity);
        }

        public void Deactivate()
        {
            if (!IsValid)
                return;
            
            world.Deactivate(entity);
        }
    }
}