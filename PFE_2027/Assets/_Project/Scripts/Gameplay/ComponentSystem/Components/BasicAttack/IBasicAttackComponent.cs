using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface IBasicAttackComponent<in TData> : 
        IComponent<TData> where TData : BasicAttackComponentData
    {
        [AddToContainer]
        void ExecuteBasicAttack(TData data);
        
        void IComponent<TData>.Trigger(TData data, ComponentContext context)
        {
            if(CanTrigger(data, context))
                ExecuteBasicAttack(data);
        }
    }
}
