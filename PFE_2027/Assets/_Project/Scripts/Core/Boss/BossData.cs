using PFE.Core.Scripts;
using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.GameSettings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace PFE.Core
{
    public abstract class BossData : GameDatabaseObject, IBossData
    {
        [field : SerializeField, BoxGroup("Description")]
        public string Name { get; private set; }

        [SerializeField, BoxGroup] 
        private StageStat<BossSpecificity> specificities;
        public StageStat<BossSpecificity> Specificities => specificities;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Specificities.EnsureSize();
        }
#endif
    }
}
