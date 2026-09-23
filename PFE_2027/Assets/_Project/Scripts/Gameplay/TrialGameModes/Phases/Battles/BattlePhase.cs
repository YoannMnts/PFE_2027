using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.GameSettings;
using PFE.Core.Scripts.Templates;
using PFE.Gameplay.Scripts.ArenaSystem;
using PFE.Gameplay.Scripts.GameModes;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Pool;
using EnemyData = PFE.Core.EnemyData;

namespace PFE.Gameplay.Scripts.Phases
{
    public class BattlePhase : Phase<bool>
    {
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        
        private readonly CrossRoadGameModeContext gameModeContext;
        private readonly SceneReference sceneToLoad;
        private ChannelKey key;
        private IAreaData currentArea;

        public BattlePhase(CrossRoadGameModeContext gameModeContext, IAreaData currentArea) : this(gameModeContext)
        {
            this.currentArea = currentArea;
        }
        
        public BattlePhase(CrossRoadGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.Default, false);
            }
            return base.Initialize(token);
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            var generateArenaPhase = new GenerateAreaPhase(currentArea);
            await generateArenaPhase.Run();
        
            var generateEnemyPhase = new GenerateEnemyPhase();
            await generateEnemyPhase.Run();
            
            //TODO créer un context de battlePhase
            while (true)
            {
                AliveEnemyPhase aliveEnemyPhase = new AliveEnemyPhase();
                PhaseResult<EnemyData> aliveEnemyResult = await aliveEnemyPhase.Run();

                return aliveEnemyResult.value;
            }

            return false;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            return base.Dispose(token);
        }

        private EnemyData GetRandomEnemy()
        {
            var allEnemyData = GameController.GameDatabase.GetAll<EnemyData>();
            var enemyArray = allEnemyData.ToArray();
            var randomIndex = Random.Range(0, enemyArray.Length);
            
            return enemyArray[randomIndex];
        }
    }
}