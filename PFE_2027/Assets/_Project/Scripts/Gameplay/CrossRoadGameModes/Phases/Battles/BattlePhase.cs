using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eflatun.SceneReference;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts;
using PFE.Core.Scripts.Area;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;
using EnemyData = PFE.Core.Scripts.Enemy.EnemyData;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class BattlePhase : Phase<bool>
    {
        private readonly CrossRoadGameModeContext gameModeContext;
        private ChannelKey key;
        
        public IEnumerable<IPlayer> Players => gameModeContext.gameMode.Players;
        
        private readonly IAreaData currentArea;

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
            var cleanupPhase = new CleanupPhase(gameModeContext);
            await cleanupPhase.Run();
            
            var generateAreaPhase = new GenerateAreaPhase(gameModeContext, currentArea);
            var arenaResult = await generateAreaPhase.Run();
        
            var generateNpcPhase = new GenerateNpcPhase(gameModeContext, currentArea, arenaResult.value);
            await generateNpcPhase.Run();

            if (!gameModeContext.NpcManager.TryGetFirstData(out PilgrimData pilgrimData))
            {
                return false;
            }
            bool result = false;
            
            if (gameModeContext.NpcManager.TryGetInstance(pilgrimData, out INpcInstance npcInstance)
                && npcInstance is PilgrimInstance pilgrimInstance)
            {
                var protectPilgrimPhase = new ProtectPilgrimPhase(gameModeContext, pilgrimInstance);
                result = await protectPilgrimPhase.Run();
            }
            
            return result;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            
            
            
            
            return base.Dispose(token);
        }
    }
}