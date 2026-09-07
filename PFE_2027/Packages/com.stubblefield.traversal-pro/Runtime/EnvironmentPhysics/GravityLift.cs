using System;
using System.Collections.Generic;
using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Lifts dynamic rigidbodies that enter its Capsule Collider trigger and moves them along the capsule. A Capsule Collider
    /// is required to be attached to this GameObject.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Environment Physics/Gravity Lift")]
    [RequireComponent(typeof(CapsuleCollider))]
    [SelectionBase]
    public class GravityLift : MonoBehaviour
    {
        [Tooltip("The speed at which dynamic rigidbodies will be moved along the Gravity Lift.")]
        public float speedGoal = 25;
        [Tooltip("The acceleration at which dynamic rigidbodies will speed up to the speed goal while inside " +
                 "this Gravity Lift.")]
        [Min(0)] public float acceleration = 25;
        [Tooltip("The maximum acceleration to apply to an object to move it towards the center of this gravity lift.")]
        [Min(0)] public float maxCenteringAcceleration = 5;
        [Tooltip("Approximately how long in seconds it will take an object to move towards the center of this " +
                 "gravity lift.")]
        [Min(0)] public float centeringSmoothTime = .5f;
        [Tooltip("Should this gravity lift rotate objects towards their default rotation?")]
        public bool centerRotation;
        [Tooltip("Approximately how long in seconds it will for an object to rotate towards its default rotation while " +
                 "in this gravity lift.")]
        [Min(0)] public float rotationSmoothTime = 1;
        CapsuleCollider capsuleCollider;
        readonly HashSet<Rigidbody> done = new();

        void OnValidate()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = true;
        }

        void Awake()
        {
            OnValidate();
        }

        void FixedUpdate()
        {
            done.Clear();
        }

        void OnTriggerStay(Collider other)
        {
            if (!enabled) return;
            
            Rigidbody otherRb = other.attachedRigidbody;
            if (!otherRb || otherRb.isKinematic || done.Contains(otherRb)) return;
            
            done.Add(otherRb);
            if ((otherRb.constraints & RigidbodyConstraints.FreezePosition) == 0)
            {
                Vector3 velocity = otherRb.linearVelocity;
                Vector3 axis = GetAxis();
                if (maxCenteringAcceleration > 0)
                {
                    Vector3 center = transform.position + capsuleCollider.center;
                    Vector3 offset = otherRb.position - center;
                    Vector3 offsetRejection = Rejection(offset, axis);
                    Vector3 velocityProjection = Vector3.Project(velocity, axis);
                    Vector3 velocityRejection = velocity - velocityProjection;
                    Vector3 lateralAcceleration = SmoothDampAcceleration(
                        offsetRejection,
                        default,
                        velocityRejection,
                        centeringSmoothTime,
                        float.PositiveInfinity,
                        Time.deltaTime);
                    lateralAcceleration = Vector3.ClampMagnitude(lateralAcceleration, maxCenteringAcceleration);
                    velocity += lateralAcceleration * Time.deltaTime;
                }

                float speedProjection0 = Vector3.Dot(velocity, axis);
                float speedProjection1 = Mathf.MoveTowards(speedProjection0, speedGoal, acceleration * Time.deltaTime);
                float speedProjectionDelta = speedProjection1 - speedProjection0;
                velocity += axis * speedProjectionDelta;
                velocity -= GetGravity(otherRb, otherRb.GetComponent<IFreeFall>()) * Time.deltaTime;
                otherRb.SetVelocity(velocity);
            }

            if (centerRotation && (otherRb.constraints & RigidbodyConstraints.FreezeRotation) == 0)
            {
                otherRb.AddTorque(SmoothDampAcceleration(otherRb.rotation, Quaternion.identity, otherRb.angularVelocity, rotationSmoothTime, float.MaxValue, Time.deltaTime), ForceMode.Acceleration);
            }
        }

        Vector3 GetAxis()
        {
            return capsuleCollider.direction switch
            {
                0 => transform.right,
                1 => transform.up,
                2 => transform.forward,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}