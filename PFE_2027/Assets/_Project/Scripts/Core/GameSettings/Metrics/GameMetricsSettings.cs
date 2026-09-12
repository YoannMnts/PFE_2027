using System;
using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Balancing"), GameSettingsPath("PFE/Game Balancing")]
    public class GameMetricsSettings : GameSettings<GameMetricsSettings>
    {
        [field: SerializeField, Range(0, 1), BoxGroup("Global")]
        public float StatisticMultiplier { get; private set; }

        [field: SerializeField, Range(1, 100), BoxGroup("Global")]
        public int StageCount { get; private set; } = 1;

        [SerializeField, BoxGroup("Boss")]
        private StageMetric<int> aggressiveness;
        public StageMetric<int> Aggressiveness => aggressiveness;

        public static event Action OnBalancingChanged;

#if UNITY_EDITOR
        private void OnValidate()
        {
            aggressiveness.EnsureSize();
            OnBalancingChanged?.Invoke();
        }
#endif
    }
}
