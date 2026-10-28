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
            private readonly IAreaData battleData;
            private readonly CampfireAreaData campfireData;

            private bool isBattlePhase;

            public DebugGameMode(IAreaData battleData, CampfireAreaData campfireData, bool isBattlePhase) : 
                base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path))
            {
                this.battleData = battleData;
                this.campfireData = campfireData;
                this.isBattlePhase = isBattlePhase;
            }
            
            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new CrossRoadGameModeContext(this);
                await GameController.GameSceneController.HideLoadingScreen();


                while (true)
                {
                    if (!isBattlePhase)
                    {
                        var campfirePhase = new CampfirePhase(context, campfireData);
                        await campfirePhase.Run();
                        isBattlePhase = true;
                    }
                    var battlePhase = new BattlePhase(context, battleData);
                    await battlePhase.Run();
                    isBattlePhase = false;
                }
            }
        }
        
        
        
        [SerializeField, BoxGroup("Parameters")]
        private bool launchOnStart = true;
        [SerializeField, BoxGroup("Parameters")] 
        private bool isBattlePhase;
        
        [SerializeField, BoxGroup("Area")]
        private AreaData battleAreaData;
        [SerializeField, BoxGroup("Area")]
        private CampfireAreaData campfireAreaData;
        
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
            
            DebugGameMode gameMode = new DebugGameMode(battleAreaData, campfireAreaData, isBattlePhase);
            
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}