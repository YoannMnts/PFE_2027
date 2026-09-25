using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core.Scripts.Enemy
{
    public interface IEnemyData : IData
    {
        public string Name { get; }
    }
}