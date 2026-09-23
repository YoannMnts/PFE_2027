using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.Enemy.Attacks;

namespace PFE.Core.Scripts.Attitude
{
    public interface IAttitudeData : IData
    {
        public AttackData[] AttacksDatas { get; }
        
        
    }
}