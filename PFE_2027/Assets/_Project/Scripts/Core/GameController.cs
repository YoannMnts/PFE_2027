using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using UnityEngine;

namespace PFE.Core.Scripts
{
    public static class GameController
    {
        public static GameSceneController GameSceneController { get; private set; }
        public static GameModeController GameModeController { get; private set; }
        public static GameDatabase GameDatabase { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GameSceneController = new GameSceneController();
            GameModeController = new GameModeController();
            GameDatabase = new GameDatabase();
        }
    }
}