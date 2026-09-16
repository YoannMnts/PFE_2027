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
using BossData = PFE.Core.BossData;

namespace PFE.Gameplay.Scripts.Phases
{
    public class BattlePhase : Phase<bool>
    {
        public IEnumerable<IPlayer> Players => gameModeContext.trialGameMode.Players;
        
        private readonly TrialGameModeContext gameModeContext;
        private readonly SceneReference sceneToLoad;

        private BossData currentBoss;


        //Only to force a boss spawn
        public BattlePhase(TrialGameModeContext gameModeContext, BossData debugData) : this(gameModeContext)
        {
            currentBoss = debugData;
        }
        
        public BattlePhase(TrialGameModeContext gameModeContext)
        {
            this.gameModeContext = gameModeContext;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            currentBoss ??= GetRandomBoss();
            return base.Initialize(token);
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            //TODO créer un context de battlePhase
            while (currentBoss != null)
            {
                BossInstance instance = new BossInstance(currentBoss);
                var battleContext = new BattleContext(this, instance);
                
                var fightPhase = new FightPhase(battleContext);
                var fightResult = await fightPhase.Run();
                BossData previousBoss = fightResult.value;
                
                var selectBossPhase = new SelectBossPhase(previousBoss);
                var selectBossResult = await selectBossPhase.Run();
                currentBoss = selectBossResult.value;
                
                var composeBuildPhase = new ComposeBuildPhase(gameModeContext);
                await composeBuildPhase.Run();
            }

            return false;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            return base.Dispose(token);
        }

        private BossData GetRandomBoss()
        {
            var allBossData = GameController.GameDatabase.GetAll<BossData>();
            var bossArray = allBossData.ToArray();
            var randomIndex = Random.Range(0, bossArray.Length);
            
            return bossArray[randomIndex];
        }
    }
}