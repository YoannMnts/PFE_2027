using PFE.Core.Scripts.Attacks;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core.Scripts.AIPattern
{
    public interface IAttitudeData : IData
    {
        public AttackData[] AttacksData { get; }
        
        
    }
}