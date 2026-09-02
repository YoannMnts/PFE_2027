using System;
using Eflatun.SceneReference;
using Helteix.Tools.Settings;
using UnityEngine;

namespace PFE.Core.Scripts.GameSettings
{
    [Serializable, AutoGenerateGameSettings, GameSettingsTitle("Scenes"), GameSettingsPath("PFE/Game Scenes")]
    public class GameSceneSettings : GameSettings<GameSceneSettings>
    {
        [field: SerializeField]
        public SceneReference  MainMenu { get; private set; }
        
        [field: SerializeField]
        public SceneReference  Game { get; private set; }
        
        [field: SerializeField]
        public SceneReference  TrainingRoom { get; private set; }
        
        [field: SerializeField]
        internal SceneLoaderUI LoaderPrefab { get; private set; }
    }
}
