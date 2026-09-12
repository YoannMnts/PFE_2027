using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using UnityEngine;
#if UNITY_EDITOR
using PFE.Core.Scripts.GameSettings;
using UnityEditor;
#endif

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract class ComponentData : GameDatabaseObject, IData
    {
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

        protected virtual void RefreshStageStats() { }
#endif
    }
}
