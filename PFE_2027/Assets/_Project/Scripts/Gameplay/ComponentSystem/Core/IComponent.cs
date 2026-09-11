using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface IComponent<in TData> : IBehaviour<TData> where TData : ComponentData
    {
        [AddToContainer]
        public bool CanTrigger(TData data, ComponentContext context);

        [AddToContainer]
        public void Trigger(TData data, ComponentContext context);
    }
}
