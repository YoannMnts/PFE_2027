using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Applies downward acceleration to the character when close to the ground to prevent the character from briefly
    /// free falling when running over changing slopes, such as when running up or down ramps or stairs.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Character Physics/Ground Snap")]
    [RequireComponent(typeof(ICharacterMotor))]
    public class GroundSnap : MonoBehaviour
    {
        [Tooltip("The max distance from the ground that snapping acceleration will be applied.")]
        [Min(0)] public float maxDistance = .2f;
        [Tooltip("The downward acceleration applied to the character when snapping to the ground.")]
        [Min(0)] public float acceleration = 15;
        public InterfaceRef<IFreeFall> freeFall;
        public InterfaceRef<IJump> jump;
        InterfaceRef<ICharacterMotor> characterMotor;
        Rigidbody groundRigidbody;
        IFreeFall groundFreeFall;
        const float recentJumpDuration = .2f;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            freeFall.Value ??= GetComponent<IFreeFall>();
            jump.Value ??= GetComponent<IJump>();
        }

        void Awake()
        {
            if (!TryValidateRequiredComponent(this, ref characterMotor)) enabled = false;
        }

        void FixedUpdate()
        {
            if (characterMotor.Value.Ground.Collider && characterMotor.Value.GroundDistance > maxDistance) return;
            if (jump.Value != null && Time.timeAsDouble < jump.Value.LastJumpTime + recentJumpDuration) return;
            if (characterMotor.Value.IsOnSteepSlope) return;
            if (!IsFreeFalling(characterMotor.Value.Rigidbody, characterMotor.Value.Acceleration, freeFall.Value)) return;
            if (groundRigidbody != characterMotor.Value.Ground.Rigidbody)
            {
                groundRigidbody = characterMotor.Value.Ground.Rigidbody;
                groundFreeFall = groundRigidbody ? groundRigidbody.GetComponent<IFreeFall>() : null;
            }
            if (!groundRigidbody || !IsFreeFalling(groundRigidbody, characterMotor.Value.Ground.Acceleration, groundFreeFall))
            {
                characterMotor.Value.Rigidbody.AddForce(new Vector3(0, -acceleration, 0), ForceMode.Acceleration);
            }
        }
    }
}