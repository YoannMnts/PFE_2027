using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core.Scripts.Attacks
{
    [GenerateContainer]
    public interface IAttack<in TData> : IBehaviour<TData> where TData : IAttackData
    {
        [AddToContainer]
        void CanAttack(TData data);
        
        [AddToContainer]
        void Execute(TData data);
    }
}