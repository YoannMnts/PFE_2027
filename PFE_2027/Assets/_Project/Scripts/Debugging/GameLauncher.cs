using System;
using System.Collections.Generic;
using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.GameModes;
using PFE.Gameplay.Scripts.Phases;
using UnityEngine;

namespace _Project.Scripts.Debugging
{
    public class GameLauncher : MonoBehaviour
    {
        private class DebugGameMode : GameMode<object>
        {
            protected override async Awaitable<object> Execute(CancellationToken token)
            {
                var battlePhase = new BattlePhase();
                return await battlePhase.Run();
            }
        }
        
        [SerializeField]
        private bool launchOnStart = true;
        
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
            
            var gameMode = new DebugGameMode();
            gameModeController.StartGameMode(gameMode);
            gameMode.RunAndForget();
        }
    }
}