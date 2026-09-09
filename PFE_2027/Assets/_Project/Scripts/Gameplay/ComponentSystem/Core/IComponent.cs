using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface IComponent<in TData> : IBehaviour<TData> where TData : ComponentData
    {
        [AddToContainer]
        public bool Trigger(TData data, ComponentContext context);
        
        [AddToContainer]
        public void StartRecharge(TData data, DurationController durationController)
            => durationController.AddOrRemove(data.MaxRechargeValue);
        
        [AddToContainer]
        public void DecrementRechargeCount(TData data, DurationController durationController)
            => durationController.AddOrRemove(data.DecrementValue);
        
    }
}