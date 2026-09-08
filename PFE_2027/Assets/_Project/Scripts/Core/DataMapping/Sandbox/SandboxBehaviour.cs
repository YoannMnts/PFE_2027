using UnityEngine;

namespace PFE.Core.DataMapping.Sandbox
{
    public partial struct SandboxBehaviour : ISandboxBehaviour<SandboxData>
    {
        public void Execute(SandboxData data)
        {
            Debug.Log($"[DataMapping Sandbox] Execute called - ID={data.ID}, Value={data.Value}");
        }
    }
}
