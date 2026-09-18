using System.Collections.Generic;
using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Gameplay.Scripts.Players;
using UnityEditor.Search;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class FightPhase : Phase<BossData>
    {
        private readonly BattleContext context;
        private ChannelKey key;
        public BossInstance CurrentBoss => context.instance;

        public FightPhase(BattleContext context)
        {
            this.context = context;
        }

        protected override Awaitable Initialize(CancellationToken token)
        {
            key = ChannelKey.GetUniqueChannelKey();
            foreach (var player in context.phase.Players)
            {
                player.ShowUI.AddPriority(key, PriorityTags.Default, false);
            }
            return base.Initialize(token);
        }

        protected override async Awaitable<BossData> Execute(CancellationToken token)
        {
            var generateArenaPhase = new GenerateArenaPhase(CurrentBoss);
            await generateArenaPhase.Run();
            
            var generateBossPhase = new GenerateBossPhase(CurrentBoss);
            await generateBossPhase.Run();
            
            AliveBossPhase aliveBossPhase = new AliveBossPhase();
            PhaseResult<BossData> aliveBossResult = await aliveBossPhase.Run();

            return aliveBossResult.value;
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            foreach (var player in context.phase.Players)
            {
                player.ShowUI.RemovePriority(key);
            }
            
            return base.Dispose(token);
        }
    }
}