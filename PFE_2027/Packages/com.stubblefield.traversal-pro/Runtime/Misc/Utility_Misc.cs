using System.Collections.Generic;
using UnityEngine;

namespace TraversalPro
{
    public static partial class Utility
    {
        internal static void ClampAnimationCurve(AnimationCurve curve, Vector2 min, Vector2 max)
        {
            if (curve.length == 0) return;
            for (int i = 0; i < curve.length; i++)
            {
                Keyframe key = curve[i];
                key.time = Mathf.Clamp(key.time, min.x, max.x);
                key.value = Mathf.Clamp(key.value, min.y, max.y);
                curve.MoveKey(i, key);
            }
        }
        
        internal static void RemoveAtSwapBack<T>(T[] array, int index, ref int count)
        {
            (array[index], array[count - 1]) = (array[count - 1], array[index]);
            count--;
        }
        
        internal static void Sort<T, TComparer>(IList<T> items, TComparer comparer, int maxCount = int.MaxValue) 
            where TComparer : IComparer<T>
        {
            int count = Mathf.Min(items.Count, maxCount);
            for (int i = 0; i < count - 1; i++)
            {
                bool wasSwapped = false;
                for (int j = 0; j < count - i - 1; j++)
                {
                    T a = items[j];
                    T b = items[j + 1];
                    if (comparer.Compare(a, b) > 0)
                    {
                        wasSwapped = true;
                        items[j] = b;
                        items[j + 1] = a;
                    }
                }
                if (!wasSwapped) break;
            }
        }
    }
}