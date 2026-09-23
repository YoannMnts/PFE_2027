using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Enemy;
using UnityEngine;

namespace PFE.Gameplay.Scripts.CrossRoadGameModes.Phases
{
    public class AliveEnemyPhase : PhaseCompletionSource<EnemyData>
    {
       private EnemyData data;
       
       //Pas sur du truc => a checker l'esprit tranquille
       protected override Awaitable<EnemyData> Execute(CancellationToken token)
       {
           if (data != null)
           {
               Dispose(token);
           }
           return base.Execute(token);
       }
       protected override Awaitable Dispose(CancellationToken token)
       {
           return(base.Dispose(token));
       }

       
    }
}