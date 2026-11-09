using System.Collections.Generic;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.GameModes;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CampfireGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases.Campfires;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using PFE.Gameplay.Scripts.RoadSystem;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes
{
    public class CrossRoadGameMode :  GameMode<bool>
    {
        public IEnumerable<IPlayer> Players => players.Values;
        public int PlayerCount => players.Count;
        
        public World World { get; private set; }
        public NpcManager NpcManager { get; private set; }
        
        
        private SceneReference sceneToLoad;
        private AreaData currentAreaData;
        private int startLaunchIndex = int.MaxValue;
        private Dictionary<int, IPlayer> players;
        
        public CrossRoadGameMode(SceneReference sceneToLoad, AreaData currentAreaData)
        {
            this.sceneToLoad = sceneToLoad;
            this.currentAreaData = currentAreaData;
        }
        
        protected override async Awaitable Initialize(CancellationToken token)
        {
            if (sceneToLoad is null)
            {
                sceneToLoad = GameSceneSettings.Current.Game;
            }
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(sceneToLoad);

            World = new World(64);
            
            players = DictionaryPool<int, IPlayer>.Get();
            players.Add(0, new DefaultPlayer(GameMetricsSettings.Current.PlayerData));
            
            NpcManager = new NpcManager();
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            /*
            CrossRoadGameModeContext context = new CrossRoadGameModeContext(this);
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
            //TODO fin du jeu => renvoie écran de fin => renvoie écran de start
            */
            return true;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            DictionaryPool<int, IPlayer>.Release(players);

            return base.Dispose(token);
        }
    }
}