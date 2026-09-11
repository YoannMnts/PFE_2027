using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Core.Scripts;
using PFE.Core.Scripts.GameSettings;
using PFE.Core.Scripts.Templates;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.Phases
{
    public class BattlePhase : Phase<bool>
    {
        private readonly ArenaData arenaData;
        private readonly SceneReference sceneToLoad;
        public IEnumerable<IPlayer> Players => players.Values;
        
        public int PlayerCount => players.Count;

        private Dictionary<int, IPlayer> players;
        
        public BattlePhase(ArenaData arenaData,SceneReference sceneToLoad = null)
        {
            this.arenaData = arenaData;
            this.sceneToLoad = sceneToLoad == null ? GameSceneSettings.Current.Game : sceneToLoad;
        }

        protected override async Awaitable Initialize(CancellationToken token)
        {
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(sceneToLoad);
            
            players = DictionaryPool<int, IPlayer>.Get();
            
            players.Add(0, new DefaultPlayer());
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            var generateArenaPhase = new GenerateArenaPhase(arenaData);
            await generateArenaPhase.Run();
            
            await GameController.GameSceneController.HideLoadingScreen();

            //TODO a changer plus tard => ABOMINATION
            BossData currentBoss = GetRandomBoss();
            //TODO créer un context de battlePhase
            while (currentBoss != null)
            {
                var fightPhase = new FightPhase(currentBoss);
                var fightResult = await fightPhase.Run();
                BossData previousBoss = fightResult.value;
                
                var selectBossPhase = new SelectBossPhase(previousBoss);
                var selectBossResult = await selectBossPhase.Run();
                currentBoss = selectBossResult.value;
                
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