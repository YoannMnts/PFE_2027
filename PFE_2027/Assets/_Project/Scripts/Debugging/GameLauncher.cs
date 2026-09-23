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
            private readonly EnemyData enemyData;

            public DebugGameMode(EnemyData enemyData) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.enemyData = enemyData;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new TrialGameModeContext(this);
                
                await GameController.GameSceneController.HideLoadingScreen();

                var startBuildPhase = new StartBuildPhase(context);
                await startBuildPhase.Run();
                
                var battlePhase = new BattlePhase(context, enemyData);
                var result = await battlePhase.Run();
            
                return result.value;
            }
        }
        
        private class DebugBattleGameMode : TrialGameMode
        {
            private readonly EnemyData enemyData;

            public DebugBattleGameMode(EnemyData enemyData) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.enemyData = enemyData;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new TrialGameModeContext(this);
                
                await GameController.GameSceneController.HideLoadingScreen();
                
                var battlePhase = new BattlePhase(context, enemyData);
                var result = await battlePhase.Run();
            
                return result.value;
            }
        }
        
        [SerializeField]
        private bool launchOnStart = true;

        [SerializeField] 
        private bool startInBattle = true;
        
        [SerializeField]
        private EnemyData enemyData;
        
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
            
            TrialGameMode gameMode = new DebugGameMode(enemyData);
            if (startInBattle)
            {
                gameMode = new DebugBattleGameMode(enemyData);
            }
            
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}