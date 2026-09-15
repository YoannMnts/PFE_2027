using System.Collections.Generic;
using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class FightPhase : Phase<BossData>
    {
        public BossInstance CurrentBoss { get; private set; }

        public FightPhase(BossInstance currentBoss)
        {
            this.CurrentBoss = currentBoss;
        }

        protected override async Awaitable<BossData> Execute(CancellationToken token)
        {
            //Pas sur du truc => a checker l'esprit tranquille
            CurrentBoss.Spawn();
            
            AliveBossPhase aliveBossPhase = new AliveBossPhase();
            PhaseResult<BossData> aliveBossResult = await aliveBossPhase.Run();

            return aliveBossResult.value;
        }
    }
}