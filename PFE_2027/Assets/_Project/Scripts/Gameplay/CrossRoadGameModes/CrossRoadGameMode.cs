using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes
{
    public class CrossRoadGameMode :  GameMode<bool>
    {
        private SceneReference sceneToLoad;
        public IEnumerable<IPlayer> Players => players.Values;
        public int PlayerCount => players.Count;

        private Dictionary<int, IPlayer> players;
        
        public CrossRoadGameMode(SceneReference sceneToLoad)
        {
            this.sceneToLoad = sceneToLoad;
        }
        
        protected override async Awaitable Initialize(CancellationToken token)
        {
            sceneToLoad ??= GameSceneSettings.Current.Game;
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(sceneToLoad);

            players = DictionaryPool<int, IPlayer>.Get();
            
            players.Add(0, new DefaultPlayer());
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            var context = new CrossRoadGameModeContext(this);
            
            await GameController.GameSceneController.HideLoadingScreen();
            
            var battlePhase = new BattlePhase(context);
            var result = await battlePhase.Run();
            
            return result.value;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            DictionaryPool<int, IPlayer>.Release(players);

            return base.Dispose(token);
        }
    }
}