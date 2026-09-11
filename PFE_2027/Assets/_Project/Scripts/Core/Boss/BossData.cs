using PFE.Core.Scripts;
using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.GameSettings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        private void OnEnable() => GameBalancingSettings.OnBalancingChanged += HandleBalancingChanged;
        private void OnDisable() => GameBalancingSettings.OnBalancingChanged -= HandleBalancingChanged;

        private void HandleBalancingChanged()
        {
            RefreshStageStats();
            EditorUtility.SetDirty(this);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            RefreshStageStats();
        }

        protected virtual void RefreshStageStats() => specificities.EnsureSize();
#endif
    }
}
