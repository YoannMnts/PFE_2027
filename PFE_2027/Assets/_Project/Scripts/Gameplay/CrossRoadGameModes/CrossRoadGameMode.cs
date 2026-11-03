using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.NPCs;
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

        private AreaData[] currentAreaDatas;
        public bool isGameFinished {get; private set;}
        
        public NpcManager NpcManager { get; private set; }
        
        public CrossRoadGameMode(SceneReference sceneToLoad)
        {
            this.sceneToLoad = sceneToLoad;
        }
        
        protected override async Awaitable Initialize(CancellationToken token)
        {
            if (sceneToLoad is null)
            {
                sceneToLoad = GameSceneSettings.Current.Game;
            }

            var areaDatas = GameMetricsSettings.Current.AreaDatas;
            currentAreaDatas = areaDatas;
            
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(sceneToLoad);

            players = DictionaryPool<int, IPlayer>.Get();
            players.Add(0, new DefaultPlayer(GameMetricsSettings.Current.PlayerData));
            
            NpcManager = new NpcManager();
            isGameFinished =  false;
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            CrossRoadGameModeContext context = new CrossRoadGameModeContext(this);
            await GameController.GameSceneController.HideLoadingScreen();

            while (!isGameFinished)
            {
                
            }
            
            return isGameFinished;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            DictionaryPool<int, IPlayer>.Release(players);

            return base.Dispose(token);
        }
    }
}