using PFE.Core.Scripts;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.GameSettings;

namespace PFE.Core
{
    public interface IBossData : IData
    {
        public string Name { get; }
    }
}