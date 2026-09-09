using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface IElementComponent<in TData> : 
        IComponent<TData> where TData : ElementComponentData
    {
        [AddToContainer]
        void ApplyElement(TData data);

        void IComponent<TData>.Trigger(TData data, ComponentContext context)
        {
            if(CanTrigger(data, context))
                ApplyElement(data);
        }
    }
}
