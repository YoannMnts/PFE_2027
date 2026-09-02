using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Local;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.Phases
{
    public class BattlePhase : Phase<bool>
    {
        public IEnumerable<IPlayer> Players => players.Values;
        
        public IPlayer CurrentPlayer => players[CurrentPlayerID];
        
        public int PlayerCount => players.Count;

        public int CurrentPlayerID { get; private set; }

        private Dictionary<int, IPlayer> players;

        
        protected override async Awaitable Initialize(CancellationToken token)
        {
            SceneReference gameScene = GameSceneSettings.Current.Game;
            await GameController.GameSceneController.LoadSceneWithLoadingScreen(gameScene);
            
            players = DictionaryPool<int, IPlayer>.Get();
            
            players.Add(0, new LocalPlayer());
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            while (true)
            {
                await Awaitable.NextFrameAsync(token);
            }
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            DictionaryPool<int, IPlayer>.Release(players);
            
            return base.Dispose(token);
        }
    }
}