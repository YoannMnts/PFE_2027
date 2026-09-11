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
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.Phases
{
    public class BattlePhase : Phase<bool>
    {
        private readonly SceneReference sceneToLoad;
        public IEnumerable<IPlayer> Players => players.Values;
        public int PlayerCount => players.Count;
        
        public BossData CurrentBoss { get; private set; }

        private Dictionary<int, IPlayer> players;

        //Only to force a boss spawn
        public BattlePhase(BossData debugData, SceneReference sceneToLoad) : this(sceneToLoad)
        {
            CurrentBoss = debugData;
        }
        
        public BattlePhase(SceneReference sceneToLoad = null)
        {
            this.sceneToLoad = sceneToLoad == null ? GameSceneSettings.Current.Game : sceneToLoad;
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(sceneToLoad);
            
            players = DictionaryPool<int, IPlayer>.Get();
            
            players.Add(0, new DefaultPlayer());
            CurrentBoss = CurrentBoss == null ? GetRandomBoss() : CurrentBoss;
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            await GameController.GameSceneController.HideLoadingScreen();

            
            //TODO créer un context de battlePhase
            while (CurrentBoss != null)
            {
                var fightPhase = new FightPhase(CurrentBoss);
                var fightResult = await fightPhase.Run();
                BossData previousBoss = fightResult.value;
                
                var selectBossPhase = new SelectBossPhase(previousBoss);
                var selectBossResult = await selectBossPhase.Run();
                CurrentBoss = selectBossResult.value;
                
                var composeBuildPhase = new ComposeBuildPhase();
                await composeBuildPhase.Run();
            }

            return false;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            DictionaryPool<int, IPlayer>.Release(players);
            
            return base.Dispose(token);
        }

        private BossData GetRandomBoss()
        {
            //TODO logic pour choisir le premier boss => à compléter 
            return null;
        }
    }
}