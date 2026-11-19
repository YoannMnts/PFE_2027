using PFE.Core.Scripts.GameSettings;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace PFE.Gameplay.Scripts.BulletTimes
{
    public static partial class TimeScaleManager
    {
        [AutoStaticsCleanup]
        public static float TimeMultiplier { get ; private set;} = GameMetricsSettings.Current.DefaultTimeScale;

        
        public static void SetTimeScale(float timeScale)
        {
            TimeMultiplier = timeScale;
        }

        public static float WithTimeScale(this float value)
        {
            return value * TimeMultiplier;
        }
        
        public static int WithTimeScale(this int value)
        {
            return Mathf.RoundToInt(value * TimeMultiplier);
        }
        
        public static Vector3 WithTimeScale(this Vector3 value)
        {
            return value * TimeMultiplier;
        }
    }
}