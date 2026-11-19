using System.Threading;
using Helteix.ChanneledProperties;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.GameSettings;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace PFE.Gameplay.Scripts.BulletTimes
{
    public class BulletTimePhase : PhaseCompletionSource<bool>
    {
        protected override Awaitable Initialize(CancellationToken token)
        {
            TimeScaleManager.SetTimeScale(GameMetricsSettings.Current.BulletTimeScale);
            return base.Initialize(token);
        }

        protected override Awaitable Dispose(CancellationToken token)
        {
            TimeScaleManager.SetTimeScale(GameMetricsSettings.Current.DefaultTimeScale);
            return base.Dispose(token);
        }
    }
}