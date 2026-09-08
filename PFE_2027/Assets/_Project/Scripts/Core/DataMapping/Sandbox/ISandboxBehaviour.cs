using PFE.Core.DataMapping;

namespace PFE.Core.DataMapping.Sandbox
{
    [GenerateContainer]
    public interface ISandboxBehaviour<TData> : IBehaviour<TData> where TData : ISandboxData
    {
        [AddToContainer]
        void Execute(TData data);
    }
}
