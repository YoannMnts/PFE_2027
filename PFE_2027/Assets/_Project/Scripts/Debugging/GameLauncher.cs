using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using Helteix.Tools.Settings;
using NUnit.Framework;
using PFE.Core;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using PFE.Gameplay.Scripts.RoadSystem;
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
            private AreaData currentAreaData;
            
            private int startLaunchIndex = int.MaxValue;

            public DebugGameMode(AreaData currentAreaData) : base(SceneReference.FromScenePath(SceneManager.GetActiveScene().path), currentAreaData)
            {
                this.currentAreaData = currentAreaData;
            }
            
            protected override async Awaitable<bool> Execute(CancellationToken token)
            {
                var context = new CrossRoadGameModeContext(this);
                await GameController.GameSceneController.HideLoadingScreen();
    
                for (var i = 0; i < GameMetricsSettings.Current.AreaDatas.Length; i++)
                { 
                    AreaData areaData = GameMetricsSettings.Current.AreaDatas[i];
                    
                    if (areaData == currentAreaData)
                    {
                        startLaunchIndex = i;
                    }

                    if (i >= startLaunchIndex)
                    {
                        currentAreaData = areaData;
                        
                        if (currentAreaData.TryGet(out IAreaContainer container))
                        {
                            await container.LaunchPhase(currentAreaData, context);
                        }
                    }
                }

                return true;
            }
        }
        
        //-----------------------------------------------------------
        
        [SerializeField, BoxGroup("Parameters")]
        private bool launchOnStart = true;
        
        [field : SerializeField, BoxGroup("Area")]
        private AreaData currentArea;
        
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
            
            DebugGameMode gameMode = new DebugGameMode(currentArea);
            
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}