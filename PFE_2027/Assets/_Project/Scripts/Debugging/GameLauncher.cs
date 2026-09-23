using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.GameModes;
using PFE.Gameplay.Scripts.ArenaSystem;
using PFE.Gameplay.Scripts.GameModes;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace PFE.Debugging._Project.Scripts.Debugging
{
    public class GameLauncher : MonoBehaviour
    {
        private class DebugGameMode : CrossRoadGameMode
        {
            private readonly IAreaData data;

            public DebugGameMode(IAreaData data) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.data = data;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new CrossRoadGameModeContext(this);
                
                await GameController.GameSceneController.HideLoadingScreen();
                
                var battlePhase = new BattlePhase(context, data);
                var result = await battlePhase.Run();
            
                return result.value;
            }
        }
        
        [SerializeField]
        private bool launchOnStart = true;
        
        [SerializeField]
        private AreaData areaData;
        
        private void Start()
        {
            if(launchOnStart)
                LaunchDebugGameMode();
        }

        private void LaunchDebugGameMode()
        {
            var gameModeController = GameController.GameModeController;
            if (gameModeController.Current != null) 
                return;
            
            CrossRoadGameMode gameMode = new DebugGameMode(areaData);
            
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}