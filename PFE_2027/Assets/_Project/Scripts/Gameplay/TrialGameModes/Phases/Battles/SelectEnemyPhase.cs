using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class SelectEnemyPhase :  Phase<EnemyData>
    {
        private readonly EnemyData previousEnemy;

        public SelectEnemyPhase(EnemyData previousEnemy)
        {
            this.previousEnemy = previousEnemy;
        }

        protected override async Awaitable<EnemyData> Execute(CancellationToken token)
        {
            await Awaitable.MainThreadAsync();
            return null;
        }
    }
}