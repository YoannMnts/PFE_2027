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
        private class DebugGameMode : TrialGameMode
        {
            private readonly BossData bossData;

            public DebugGameMode(BossData bossData) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.bossData = bossData;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new BattleGameModeContext(this);
                
                await GameController.GameSceneController.HideLoadingScreen();

                var startBuildPhase = new StartBuildPhase(context);
                await startBuildPhase.Run();
                
                var battlePhase = new BattlePhase(context, bossData);
                var result = await battlePhase.Run();
            
                return result.value;
            }
        }
        
        private class DebugBattleGameMode : TrialGameMode
        {
            private readonly BossData bossData;

            public DebugBattleGameMode(BossData bossData) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.bossData = bossData;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new BattleGameModeContext(this);
                
                await GameController.GameSceneController.HideLoadingScreen();
                
                var battlePhase = new BattlePhase(context, bossData);
                var result = await battlePhase.Run();
            
                return result.value;
            }
        }
        
        [SerializeField]
        private bool launchOnStart = true;

        [SerializeField] 
        private bool startInBattle = true;
        
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
            
            TrialGameMode gameMode = new DebugGameMode(bossData);
            if (startInBattle)
            {
                gameMode = new DebugBattleGameMode(bossData);
            }
            
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}