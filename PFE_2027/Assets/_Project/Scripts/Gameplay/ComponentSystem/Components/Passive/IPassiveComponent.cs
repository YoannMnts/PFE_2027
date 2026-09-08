using PFE.Core.Scripts.ComponentSystem;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    [GenerateContainer]
    public interface IPassiveComponent<in TData> : IBehaviour<TData> where TData : ComponentData
    {
        [AddToContainer]
        void ApplyPassive(TData data);
    }
}
