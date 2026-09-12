using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable]
    public struct StageMetric<T>
    {
        [SerializeField, ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)] 
        private T[] stages;

        public readonly T GetValue(int stageIndex)
        {
            if (stages == null || stages.Length == 0)
            {
                Debug.LogError($"[StageStat<{typeof(T).Name}>] Tableau non initialisé — as-tu bien appelé EnsureSize() dans l'OnValidate() du propriétaire ?");
                return default;
            }

#if UNITY_EDITOR
            int expected = GameMetricsSettings.Current.StageCount;
            if (stages.Length != expected)
                Debug.LogWarning($"[StageStat<{typeof(T).Name}>] Taille ({stages.Length}) désynchronisée avec StageCount ({expected}) — vérifie que EnsureSize() est bien appelé dans l'OnValidate() du propriétaire.");
#endif

            return stages[Mathf.Clamp(stageIndex, 0, stages.Length - 1)];
        }

#if UNITY_EDITOR
        public void EnsureSize()
        {
            int count = GameMetricsSettings.Current.StageCount;
            if (stages == null || stages.Length != count)
                Array.Resize(ref stages, count);
        }
#endif
    }
}
