using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class FightPhase :  Phase<BossData>
    {
        private readonly BossData currentBoss;

        public FightPhase(BossData currentBoss)
        {
            this.currentBoss = currentBoss;
        }

        protected override async Awaitable<BossData> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return null;
        }
    }
}