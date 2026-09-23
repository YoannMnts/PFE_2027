using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core.Scripts.Attitude
{
    [GenerateContainer]
    public interface IAttitude<in TData> : IBehaviour<TData> where TData : IAttitudeData
    {
        [AddToContainer]
        void CalculateAccuracy(TData data);
    }
}