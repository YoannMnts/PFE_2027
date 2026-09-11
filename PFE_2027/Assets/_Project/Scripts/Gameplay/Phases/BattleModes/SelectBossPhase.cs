using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class SelectBossPhase :  Phase<BossData>
    {
        private readonly BossData previousBoss;

        public SelectBossPhase(BossData previousBoss)
        {
            this.previousBoss = previousBoss;
        }

        protected override async Awaitable<BossData> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return null;
        }
    }
}