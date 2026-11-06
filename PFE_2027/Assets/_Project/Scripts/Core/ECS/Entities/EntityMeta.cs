namespace PFE.Core.Scripts.ECS
{
    public struct EntityMeta
    {
        public bool IsActive {get; private set;}
        
        public ComponentMask Mask {get; private set;}

        public EntityMeta(bool isActive)
        {
            IsActive = isActive;
            Mask = new ComponentMask();
        }

        public void AddComponent<TComponent>() where TComponent : IEntityComponent 
            => Mask.Set(ComponentID<TComponent>.Value);

        public void RemoveComponent<TComponent>() where TComponent : IEntityComponent 
            => Mask.Clear(ComponentID<TComponent>.Value);

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}