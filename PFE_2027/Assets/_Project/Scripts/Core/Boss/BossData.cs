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
        private StageMetric<BossSpecificity> metrics;
        public StageMetric<BossSpecificity> Metrics => metrics;

#if UNITY_EDITOR
        private void OnEnable() => GameMetricsSettings.OnBalancingChanged += HandleBalancingChanged;
        private void OnDisable() => GameMetricsSettings.OnBalancingChanged -= HandleBalancingChanged;

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

        protected virtual void RefreshStageStats() => metrics.EnsureSize();
#endif
    }
}
