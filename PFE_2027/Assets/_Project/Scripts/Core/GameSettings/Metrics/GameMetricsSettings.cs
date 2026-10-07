using System;
using Helteix.Tools.Settings;
using PFE.Core.Scripts.Pilgrims;
using PFE.Core.Scripts.Players;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Metrics"), GameSettingsPath("PFE/Game Metrics")]
    public class GameMetricsSettings : GameSettings<GameMetricsSettings>
    {
        // Data given to the player by default (a game mode can pass another one)
        [field: SerializeField, BoxGroup("Players")]
        public PlayerData PlayerData { get; private set; }
        
        [field: SerializeField, BoxGroup("Pilgrim")]
        public PilgrimData PilgrimData { get; private set; }

        // Rule of the "protect the pilgrim" mode: the player must stay within this radius around the pilgrim
        [field: SerializeField, Min(0.5f), BoxGroup("Pilgrim")]
        public float LeashRadius { get; private set; } = 8f;

        // Speed at which the player is pulled back when the pilgrim got too far away
        [field: SerializeField, Min(0f), BoxGroup("Pilgrim")]
        public float LeashPullBackSpeed { get; private set; } = 4f;
    }
}
