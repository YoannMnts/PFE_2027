using PFE.Core.Scripts;
using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core
{
    public abstract class BossData : GameDatabaseObject, IBossData
    {
        [field : SerializeField, BoxGroup("Parameters")]
        public string Name { get; private set; }
        
        [field : SerializeField, BoxGroup("Balance")]
        public StageBalance[] StageBalances { get; private set; }

    }
}
