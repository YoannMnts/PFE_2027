using System.Threading;
using Helteix.Tools.Phases;
using PFE.Core;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public class AliveBossPhase : PhaseCompletionSource<BossData>
    {
       private BossData data;
       
       //Pas sur du truc => a checker l'esprit tranquille
       protected override Awaitable<BossData> Execute(CancellationToken token)
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