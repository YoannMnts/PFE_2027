using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using UnityEngine;

namespace PFE.Gameplay.Scripts.BulletTimes
{
    public class BulletTimePhase : PhaseCompletionSource<bool>
    {
        public static float TimeMultiplier { get ; private set;} = GameMetricsSettings.Current.DefaultTimeScale;
        
        protected override Awaitable Initialize(CancellationToken token)
        {
            TimeMultiplier = GameMetricsSettings.Current.BulletTimeScale;
            
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            TimeMultiplier = GameMetricsSettings.Current.DefaultTimeScale;
            
            return base.Dispose(token);
        }
    }
}