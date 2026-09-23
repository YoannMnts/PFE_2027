using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts;
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
        public IEnumerable<IPlayer> Players => gameModeContext.trialGameMode.Players;
        
        private readonly TrialGameModeContext gameModeContext;
        private readonly SceneReference sceneToLoad;

        private EnemyData currentEnemy;


        //Only to force a enemy spawn
        public BattlePhase(TrialGameModeContext gameModeContext, EnemyData debugData) : this(gameModeContext)
        {
            currentEnemy = debugData;
        }
        
        public BattlePhase(TrialGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            currentEnemy ??= GetRandomEnemy();
            return base.Initialize(token);
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            //TODO créer un context de battlePhase
            while (currentEnemy != null)
            {
                var composeBuildPhase = new ComposeBuildPhase(gameModeContext);
                await composeBuildPhase.Run();
                
                EnemyInstance instance = new EnemyInstance(currentEnemy);
                var battleContext = new BattleContext(this, instance);
                
                var fightPhase = new FightPhase(battleContext);
                var fightResult = await fightPhase.Run();
                EnemyData previousEnemy = fightResult.value;
                
                var selectEnemyPhase = new SelectEnemyPhase(previousEnemy);
                var selectEnemyResult = await selectEnemyPhase.Run();
                currentEnemy = selectEnemyResult.value;
            }

            return false;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
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