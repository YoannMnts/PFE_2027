using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core.Scripts.DataMapping.Sandbox
{
    [GenerateContainer]
    public interface ISandboxBehaviour<TData> : IBehaviour<TData> where TData : ISandboxData
    {
        [AddToContainer]
        void Execute(TData data);
    }
}
