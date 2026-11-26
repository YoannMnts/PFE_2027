using System;
using Helteix.Tools.Settings;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Pilgrims;
using PFE.Core.Scripts.Players;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Game Assets"), GameSettingsPath("PFE/Game Assets")]
    public class GameAssets : GameSettings<GameAssets>
    {
        [field: SerializeField]
        public AudioSource BGMAudioSource { get; private set; }

    }
}
