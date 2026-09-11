using PFE.Core.Scripts;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core
{
    public interface IBossData : IData
    {
        public string Name { get; }
        public StageSpecificity[] StageBalances { get; }
    }
}