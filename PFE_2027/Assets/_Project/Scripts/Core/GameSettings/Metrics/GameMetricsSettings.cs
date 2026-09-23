using System;
using Helteix.Tools.Settings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Metrics"), GameSettingsPath("PFE/Game Metrics")]
    public class GameMetricsSettings : GameSettings<GameMetricsSettings>
    {
        
    }
}
