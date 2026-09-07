using UnityEngine;

namespace TraversalPro.Samples.Playground
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class CarWaypointFollower : MonoBehaviour
    {
        public WaypointSeries waypointSeries;
        public float motorTorque = 2000;
        public float maxSpeed = 5;
        public float steeringRange = 30;
        public float steeringSpeed = 100;
        public Wheel[] wheels;
        Rigidbody rb;
        float steeringVelocity;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void Update()
        {
            float forwardSpeed = Vector3.Dot(transform.forward, rb.linearVelocity);
            float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);
            float availableMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
            foreach (var wheel in wheels)
            {
                if (wheel.steerable)
                {
                    Vector3 waypointOffset = waypointSeries.Current.position - transform.position;
                    float worldDegreesGoal = Mathf.Atan2(waypointOffset.z, waypointOffset.x) * Mathf.Rad2Deg;
                    Vector3 forward = transform.forward;
                    float worldDegreesCurrent = Mathf.Atan2(forward.z, forward.x) * Mathf.Rad2Deg;
                    float localDegreesGoal = worldDegreesGoal - worldDegreesCurrent;
                    localDegreesGoal = localDegreesGoal > 180 
                        ? localDegreesGoal - 360 
                        : localDegreesGoal < -180 
                            ? localDegreesGoal + 360 
                            : localDegreesGoal;
                    float localDegreesGoalClamped = Mathf.Clamp(localDegreesGoal, -steeringRange, steeringRange);
                    wheel.WheelCollider.steerAngle = Mathf.MoveTowards(
                        wheel.WheelCollider.steerAngle,
                        -localDegreesGoalClamped,
                        steeringSpeed * Time.deltaTime);
                }
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = availableMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
        }
    }
    
    [System.Serializable]
    public struct Wheel
    {
        public WheelCollider WheelCollider;
        public bool steerable;
        public bool motorized;
    }
}