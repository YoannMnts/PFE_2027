using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class FightPhase :  Phase<BossData>
    {
        public BossData CurrentBoss { get; private set; }

        public FightPhase(BossData currentBoss)
        {
            this.CurrentBoss = currentBoss;
        }

        protected override async Awaitable<BossData> Execute(CancellationToken token)
        {
            while (true)
            {
                await Awaitable.NextFrameAsync(token);
            }
            return null;
        }
    }
}