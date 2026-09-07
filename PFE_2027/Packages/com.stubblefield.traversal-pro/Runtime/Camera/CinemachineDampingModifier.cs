using System;
using UnityEngine;
using Unity.Cinemachine;
using Unity.Mathematics;
using static TraversalPro.Utility;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TraversalPro
{
    /// <summary>
    /// Modifies the damping value on a Cinemachine component based on the velocity of an <see cref="ICharacterMotor"/>.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Camera/Cinemachine Damping Modifier")]
    public class CinemachineDampingModifier : MonoBehaviour
    {
        [Tooltip("The character the Cinemachine camera is following.")]
        public InterfaceRef<ICharacterMotor> characterMotor;
        [Tooltip("The Cinemachine component on this GameObject with a Position Damping value.")]
        public CinemachineComponentBase cinemachineComponent;
        [Tooltip("A curve function where the input is the character's speed and the output is a factor " +
                 "to apply to the Position Damping of the referenced Cinemachine component.")]
        public AnimationCurve speedToDampingFactor = new(new Keyframe(0, 1, 0, -.15f), new Keyframe(15, 0));
        [Tooltip("Approximately how long in seconds it will take the Position Damping value to reach the calculated value.")]
        [Min(0)] public float smoothTime = .2f;
        public Vector3 BaseDampingValue { get; private set; }
        float3 tGoal;
        float3 t;
        Vector3 tVelocity;
        [Obsolete, HideInInspector] public float maxFreeFallFactor;

        Vector3 Damping
        {
            get
            {
                return cinemachineComponent switch
                { 
                    CinemachineFreeLookModifier.IModifiablePositionDamping damper => damper.PositionDamping, 
                    CinemachineFollow follow => follow.TrackerSettings.PositionDamping,
                    _ => default,
                };
            }
            set
            {
                switch (cinemachineComponent)
                {
                    case CinemachineFreeLookModifier.IModifiablePositionDamping damper:
                        damper.PositionDamping = value;
                        break;
                    case CinemachineFollow follow:
                        follow.TrackerSettings.PositionDamping = value;
                        break;
                }
            }
        }

        void Reset()
        {
            OnValidate();
#if UNITY_EDITOR
            AnimationUtility.SetKeyBroken(speedToDampingFactor, 0, true);
            AnimationUtility.SetKeyBroken(speedToDampingFactor, 1, true);
#endif
        }

        void OnValidate()
        {
            characterMotor.Value ??= transform.root.GetComponentInChildren<ICharacterMotor>();
            if (!cinemachineComponent) cinemachineComponent = GetComponent<CinemachineFreeLookModifier.IModifiablePositionDamping>() as CinemachineComponentBase;
            if (!cinemachineComponent) cinemachineComponent = GetComponent<CinemachineFollow>();
            if (cinemachineComponent
                && cinemachineComponent is not CinemachineFreeLookModifier.IModifiablePositionDamping
                && cinemachineComponent is not CinemachineFollow)
            {
                LogError($"The cinemachineComponent field currently has a reference to an object which is not a known " +
                         $"Cinemachine component with a PositionDamping value.");
                cinemachineComponent = null;
            }
            ClampAnimationCurve(speedToDampingFactor, default, new Vector2(float.MaxValue, 1));
        }

        void OnEnable()
        {
            if (TryValidateRequiredField(this, characterMotor.Value)
                && TryValidateRequiredField(this, cinemachineComponent))
            {
                BaseDampingValue = Damping;
            }
            else
            {
                enabled = false;
            }
        }

        void Update()
        {
            if (Time.deltaTime <= 0) return;

            Transform cam = cinemachineComponent.VirtualCamera.transform;
            float3 localVelocity = cam.InverseTransformDirection(characterMotor.Value.Rigidbody.linearVelocity);
            float3 groundAcceleration = cam.InverseTransformDirection(characterMotor.Value.Ground.Acceleration);
            float3 tRaw = math.clamp(Evaluate(speedToDampingFactor, math.abs(localVelocity)), 0, 1);
            bool3 isGroundDecelerating = math.sign(localVelocity) * math.sign(groundAcceleration) < 0;
            bool isGrounded = characterMotor.Value.IsGrounded;
            float3 freeFallGoal = math.min(tGoal, tRaw);
            float3 deceleratingGoal = math.min(freeFallGoal, 1 - tRaw);
            tGoal = math.select(freeFallGoal, math.select(tRaw, deceleratingGoal, isGroundDecelerating), isGrounded);
            t = Vector3.SmoothDamp(t, tGoal, ref tVelocity, smoothTime);
            Damping = Vector3.Scale(BaseDampingValue, t);
        }

        static float3 Evaluate(AnimationCurve curve, float3 t)
        {
            return new float3(curve.Evaluate(t.x), curve.Evaluate(t.y), curve.Evaluate(t.z));
        }
    }
}