using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace PFE.Debugging._Project.Scripts.Debugging
{
    public class GameLauncher : MonoBehaviour
    {
        private class DebugGameMode : CrossRoadGameMode
        {
            private readonly IAreaData data;
            private readonly bool isBattlePhase;

            public DebugGameMode(IAreaData data, bool isBattlePhase) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.data = data;
                this.isBattlePhase = isBattlePhase;
            }

            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new CrossRoadGameModeContext(this);
                await GameController.GameSceneController.HideLoadingScreen();

                if (isBattlePhase)
                {
                    var battlePhase = new BattlePhase(context, data);
                    await battlePhase.Run();
                }
                var campfirePhase = new CampfirePhase(context, data);
                await campfirePhase.Run();

                return true;
            }
        }
        
        
        
        [SerializeField]
        private bool launchOnStart = true;

        [SerializeField] private bool isBattlePhase;
        
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
            
            DebugGameMode gameMode = new DebugGameMode(areaData, isBattlePhase);
            
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}