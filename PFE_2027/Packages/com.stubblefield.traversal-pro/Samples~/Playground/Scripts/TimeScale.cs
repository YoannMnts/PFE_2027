using UnityEngine;

namespace TraversalPro.Samples.Playground
{
    public class TimeScale : MonoBehaviour
    {
        [Min(0)] public float timeScale = 1;

        void OnValidate()
        {
            Time.timeScale = timeScale;
        }

        void Start()
        {
            Time.timeScale = timeScale;
        }
    }
}