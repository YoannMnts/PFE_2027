using PFE.Core.Scripts;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.GameSettings;

namespace PFE.Core
{
    public interface IEnemyData : IData
    {
        public string Name { get; }
    }
}