using UnityEngine;
using UnityEngine.InputSystem;

namespace TraversalPro.Samples.Playground
{
    public class WaypointSeries : MonoBehaviour
    {
        public Transform target;
        public float radius = 1f;
        int _currentIndex;

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                _currentIndex = value % transform.childCount;
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(i == _currentIndex);
                }
            }
        }

        public Transform Current => transform.GetChild(CurrentIndex);

        public int Count => transform.childCount;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Current.position, radius);
        }

        void Awake()
        {
            if (!target)
            {
                target = FindAnyObjectByType<PlayerInput>().transform;
            }
            if (transform.childCount < 2)
            {
                enabled = false;
                Debug.LogError($"[WaypointRace] Two or more children are required under the GameObject '{name}'.");
                return;
            }
            CurrentIndex = 0;
        }

        void Update()
        {
            if (transform.childCount < 2) return;
            for (int i = 0; IsTargetNearWaypoint() && i < 1000; i++)
            {
                CurrentIndex++;
            }
        }

        bool IsTargetNearWaypoint()
        {
            return target && Vector3.Distance(target.position, transform.GetChild(CurrentIndex).position) < radius;
        }
    }
}