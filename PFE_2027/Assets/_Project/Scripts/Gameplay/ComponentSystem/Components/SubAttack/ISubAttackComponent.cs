using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface ISubAttackComponent<in TData> : 
        IComponent<TData> where TData : SubAttackComponentData
    {
        [AddToContainer]
        void ExecuteSubAttack(TData data);
    }
}
