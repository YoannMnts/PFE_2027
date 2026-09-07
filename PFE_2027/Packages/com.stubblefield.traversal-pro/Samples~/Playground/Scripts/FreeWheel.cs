using UnityEngine;

namespace TraversalPro.Samples.Playground
{
    [RequireComponent(typeof(WheelCollider))]
    public class FreeWheel : MonoBehaviour
    {
        WheelCollider wheelCollider;

        void Awake()
        {
            wheelCollider = GetComponent<WheelCollider>();
        }

        void Update()
        {
            wheelCollider.motorTorque = .001f;
        }
    }
}