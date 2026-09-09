using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface IPassiveComponent<in TData> : 
        IComponent<TData> where TData : PassiveComponentData
    {
        [AddToContainer]
        void ApplyPassive(TData data);
        
        void IComponent<TData>.Trigger(TData data, ComponentContext context)
        {
            if(CanTrigger(data, context))
                ApplyPassive(data);
        }
    }
}
