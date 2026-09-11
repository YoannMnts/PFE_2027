using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping.Attributes;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    /// <summary>
    /// Split out of <see cref="IComponent{TData}"/> because only BasicAttack and
    /// SubAttack actually have a recharge — Element and Passive stay plain
    /// IComponent and never carry a <see cref="RechargeableComponentData"/>.
    /// </summary>
    [GenerateContainer]
    public interface IRechargeableComponent<in TData> : IComponent<TData> where TData : RechargeableComponentData
    {
        [AddToContainer]
        public void StartRecharge(TData data, DurationController durationController)
            => durationController.AddOrRemove(data.MaxRechargeValue);

        [AddToContainer]
        public void DecrementRechargeCount(TData data, DurationController durationController)
            => durationController.AddOrRemove(data.DecrementValue);
    }
}
