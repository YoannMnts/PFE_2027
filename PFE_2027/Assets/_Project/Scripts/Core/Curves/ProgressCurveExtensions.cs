using UnityEngine;

namespace PFE.Core.Scripts.Curves
{
    // Progress curves: X = seconds since the start, Y = part of the total already done. Only the shape matters:
    // values are divided by the last key, so the curve always ends at 100% whatever its last value.
    public static class ProgressCurveExtensions
    {
        // Duration of the curve (time of its last key), -1 when it can't be used as a progress curve
        public static float GetProgressDuration(this AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
                return -1f;

            Keyframe lastKey = curve[curve.length - 1];
            return lastKey.time > 0f && lastKey.value > 0f ? lastKey.time : -1f;
        }

        // Part of the total (0 to 1) done at "time"
        public static float EvaluateProgress(this AnimationCurve curve, float time)
        {
            return curve.Evaluate(time) / curve[curve.length - 1].value;
        }

        // Part of the total done between two times: what to move during one frame.
        // Summed over the whole curve it always gives exactly 1, whatever the frame rate.
        public static float EvaluateProgressDelta(this AnimationCurve curve, float fromTime, float toTime)
        {
            return (curve.Evaluate(toTime) - curve.Evaluate(fromTime)) / curve[curve.length - 1].value;
        }
    }
}
