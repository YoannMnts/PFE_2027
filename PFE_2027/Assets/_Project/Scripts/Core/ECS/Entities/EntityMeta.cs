namespace PFE.Core.Scripts.ECS
{
    public struct EntityMeta
    {
        public bool IsActive {get; private set;}

        private ComponentMask mask;
        public ComponentMask Mask => mask;

        public EntityMeta(bool isActive)
        {
            IsActive = isActive;
            mask = new ComponentMask();
        }

        public void AddComponent<TComponent>() where TComponent : IEntityComponent 
            => mask.Set(ComponentID<TComponent>.Value);

        public void RemoveComponent<TComponent>() where TComponent : IEntityComponent 
            => mask.Clear(ComponentID<TComponent>.Value);

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}