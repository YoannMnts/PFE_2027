using System;
using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Metrics"), GameSettingsPath("PFE/Game Metrics")]
    public class GameMetricsSettings : GameSettings<GameMetricsSettings>
    {
        [field: SerializeField, Range(0, 100), BoxGroup("Players")]
        public int BasicAttackDamage { get; private set; } = 10;
    }
}
