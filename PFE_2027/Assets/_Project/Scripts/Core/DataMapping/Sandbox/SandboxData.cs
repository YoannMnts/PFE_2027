using PFE.Core.Scripts.Databases;
using UnityEngine;

namespace PFE.Core.DataMapping.Sandbox
{
    [CreateAssetMenu(menuName = "PFE/Sandbox/Sandbox Data", fileName = "NewSandboxData")]
    public class SandboxData : GameDatabaseObject, ISandboxData
    {
        [SerializeField] private int value;

        public int Value => value;
    }
}
