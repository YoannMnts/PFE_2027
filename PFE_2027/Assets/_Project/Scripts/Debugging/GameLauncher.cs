using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts;
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
        private class DebugGameMode : BattleGameMode
        {
            private readonly BossData bossData;

            public DebugGameMode(BossData bossData)
            {
                this.bossData = bossData;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var battlePhase = new BattlePhase(bossData, SceneReference.FromScenePath(SceneManager.GetActiveScene().path));
                var result = await battlePhase.Run();
            
                return result.value;
            }
        }
        
        [SerializeField]
        private bool launchOnStart = true;
        
        [SerializeField]
        private BossData bossData;
        
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
            
            var gameMode = new DebugGameMode(bossData);
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}