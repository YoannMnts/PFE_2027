using System;
using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Balancing"), GameSettingsPath("PFE/Game Balancing")]
    public class GameBalancingSettings : GameSettings<GameBalancingSettings>
    {
        [field: SerializeField, Range(0, 1), BoxGroup("Global")]
        public float StatisticMultiplier { get; private set; }

        [field: SerializeField, Range(1, 100), BoxGroup("Global")]
        public int StageCount { get; private set; }

        [SerializeField, BoxGroup("Boss")]
        private StageStat<int> aggressiveness;
        public StageStat<int> Aggressiveness => aggressiveness;

#if UNITY_EDITOR
        private void OnValidate() => aggressiveness.EnsureSize();
#endif
    }
}
