using System.Collections.Generic;
using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.GameModes;
using PFE.Gameplay.Scripts.Phases;
using PFE.Gameplay.Scripts.Players;
using PFE.Gameplay.Scripts.Players.Default;
using UnityEngine;
using UnityEngine.Pool;

namespace PFE.Gameplay.Scripts.GameModes
{
    public class TrialGameMode :  GameMode<bool>
    {
        public IEnumerable<IPlayer> Players => players.Values;
        public int PlayerCount => players.Count;

        private Dictionary<int, IPlayer> players;

        protected override Awaitable Initialize(CancellationToken token)
        {
            players = DictionaryPool<int, IPlayer>.Get();
            
            players.Add(0, new DefaultPlayer());

            return base.Initialize(token);
        }

        protected override async Awaitable<bool> Execute(CancellationToken token)
        {
            var context = new BattleGameModeContext(this);
            
            var startBuildPhase = new StartBuildPhase(context);
            await startBuildPhase.Run();
            
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