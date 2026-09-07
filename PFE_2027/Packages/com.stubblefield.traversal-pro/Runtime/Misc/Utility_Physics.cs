using Unity.Mathematics;
using UnityEngine;
#if UNITY_2022
using UnityPhysicsMat = UnityEngine.PhysicMaterial;
using UnityPhysicsMatCombine = UnityEngine.PhysicMaterialCombine;
#elif UNITY_6000_0_OR_NEWER
using UnityPhysicsMat = UnityEngine.PhysicsMaterial;
using UnityPhysicsMatCombine = UnityEngine.PhysicsMaterialCombine;
#endif

namespace TraversalPro
{
    public static partial class Utility
    {
        internal const float DefaultStaticFriction = 1;
        internal const float DefaultDynamicFriction = .6f;
        
        internal static Collider ContactCollider(in ContactPoint contact, Collider ignore)
        {
            return contact.thisCollider == ignore ? contact.otherCollider : contact.thisCollider;
        }

        internal static Vector3 ContactNormal(in ContactPoint contact, Vector3 origin)
        {
            return Vector3.Dot(origin - contact.point, contact.normal) > 0 ? contact.normal : -contact.normal;
        }

        internal static UnityPhysicsMat CreateZeroFrictionZeroBounceMaterial()
        {
            UnityPhysicsMat material = new()
            {
                bounceCombine = UnityPhysicsMatCombine.Minimum,
                bounciness = 0,
                frictionCombine = UnityPhysicsMatCombine.Minimum,
                dynamicFriction = 0,
                staticFriction = 0,
            };
            material.name = "Zero Friction";
            return material;
        }
        
        internal static float DragAcceleration(float speed, float terminalSpeed, float gravity)
        {
            // terminalVelocity = sqrt((2 * mass * gravity) / (dragCoefficient * airDensity * referenceArea))
            // dragForce = (dragCoefficient * airDensity * referenceArea * fluidSpeed * fluidSpeed) / 2
            terminalSpeed = Mathf.Max(terminalSpeed, .001f);
            double terminalSpeedFraction = (double)speed / terminalSpeed;
            return Mathf.Abs((float)(gravity * terminalSpeedFraction * terminalSpeedFraction));
        }
        
        /// <param name="slopeDegrees">In degrees.</param>
        internal static float FrictionAccelerationMagnitude(float slopeDegrees, float gravity, float frictionCoefficient)
        {
            return Mathf.Abs(gravity * Mathf.Cos(slopeDegrees * Mathf.Deg2Rad) * frictionCoefficient);
        }

        public static Vector3 GetGravity(Rigidbody rb, IFreeFall freeFall)
        {
            if (!rb) return default;
            if (!rb.useGravity) return default;
            if (freeFall == null) return Physics.gravity;
            return freeFall.Gravity;
        }
        
        internal static int GetMaskWithAllDefinedLayers()
        {
            int value = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    value |= (1 << i);
                }   
            }
            return value;
        }
        
        internal static (Vector3 velocity, Vector3 acceleration) GetVelocityAcceleration(Rigidbody rb, Vector3 priorVelocity, float deltaTime)
        {
            Vector3 v = rb.linearVelocity;
            double3 velocity0 = new(priorVelocity.x, priorVelocity.y, priorVelocity.z);
            double3 velocity1 = new(v.x, v.y, v.z);
            double3 acc = (velocity1 - velocity0) / deltaTime;
            return (v, new Vector3((float)acc.x, (float)acc.y, (float)acc.z));
        }

        public static bool IsFreeFalling(Rigidbody rigidbody, Vector3 acceleration, IFreeFall freeFall, float gravityPercentThreshold = .9f)
        {
            if (freeFall != null) return freeFall.IsFreeFalling;
            if (!rigidbody.useGravity) return true;
            float gravity = Physics.gravity.y;
            float sign = Mathf.Sign(gravity);
            return acceleration.y * sign > gravity * sign * gravityPercentThreshold;
        }
        
        internal static float JumpSpeed(float constantDeceleration, float distance)
        {
            return Mathf.Sqrt(2 * Mathf.Abs(constantDeceleration * distance));
        }
        
        /// <summary>
        /// Transforms position from local space to world space.
        /// </summary>
        internal static Vector3 LocalToWorldPoint(this Rigidbody rb, Vector3 localPoint)
        {
            return rb.rotation * localPoint + rb.position;
        }

        internal static int PlayerLayerIndex()
        {
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex < 0)
            {
                playerLayerIndex = LayerMask.NameToLayer("player");
            }
            return playerLayerIndex;
        }

        internal static int PlayerLayerMask()
        {
            int playerLayerIndex = PlayerLayerIndex();
            return playerLayerIndex >= 0 ? 1 << playerLayerIndex : 0;
        }

        internal static Vector3 SlopeGravityAcceleration(Vector3 slopeNormal, float gravity)
        {
            float slopeAngle = Vector3.Angle(slopeNormal, Vector3.up) * Mathf.Deg2Rad;
            if (slopeAngle < .0001f) return default;
            Vector3 lateralAxis = Vector3.Cross(Vector3.up, slopeNormal);
            Vector3 downwardAxis = Vector3.Cross(lateralAxis, slopeNormal);
            return downwardAxis * (-gravity * Mathf.Sin(slopeAngle));
        }
        
        /// <summary>
        /// Modifies the x and z values of a velocity value based on a goal velocity and the ground.
        /// </summary>
        internal static void UpdateVelocityHorizontally(
            ref Vector3 velocity, 
            Vector2 localVelocityGoal, 
            float acceleration,
            Vector3 groundVelocity,
            float movementSlopeDegrees)
        {
            float deltaTime = Time.deltaTime;
            float flatness = Mathf.Cos(movementSlopeDegrees * Mathf.Deg2Rad);
            localVelocityGoal *= flatness;
            acceleration *= flatness;
            Vector2 localVelocity = (velocity - groundVelocity).XZ();
            Offset2 localVelocityGoalDelta = new(localVelocityGoal - localVelocity);
            bool isVelocityAtGoal = localVelocityGoalDelta.Magnitude < acceleration * deltaTime;
            bool isGoal0 = localVelocityGoal.magnitude < minVectorMagnitude;
            if (isGoal0 & isVelocityAtGoal)
            {
                velocity.x = groundVelocity.x;
                velocity.z = groundVelocity.z;
                return;
            }
            acceleration = Mathf.Min(acceleration, localVelocityGoalDelta.Magnitude / deltaTime);
            Vector2 velocityDelta = localVelocityGoalDelta.Direction * acceleration * deltaTime;
            velocity.x += velocityDelta.x;
            velocity.z += velocityDelta.y;
        }
        
        public static void SetVelocity(this Rigidbody rb, Vector3 value)
        {
#if UNITY_2022
            rb.velocity = value;
#elif UNITY_6000_0_OR_NEWER
            rb.linearVelocity = value;
#endif
        }
        
        /// <summary>
        /// Transforms position from world space to local space.
        /// </summary>
        internal static Vector3 WorldToLocalPoint(this Rigidbody rb, Vector3 worldPoint)
        {
            return Quaternion.Inverse(rb.rotation) * (worldPoint - rb.position);
        }
    }
}