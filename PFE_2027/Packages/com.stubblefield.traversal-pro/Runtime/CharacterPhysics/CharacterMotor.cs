using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Detects ground and applies forces to the attached Rigidbody to slide it along the ground.
    /// This is meant to simulate a character moving along the ground. A Rigidbody and CapsuleCollider
    /// are required on this GameObject.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Character Physics/Character Motor")]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [DefaultExecutionOrder(-1000)]
    public class CharacterMotor : MonoBehaviour, ICharacterMotor
    {
        [Tooltip("The layers that colliders in the scene can be on to be considered ground for this character.")]
        public LayerMask layerMask = 1;
        [Tooltip("The minimum angle in degrees of a slope for it to be considered a steep slope.")]
        [Min(0)] public float steepSlopeThresholdDegrees = 56;
        [Tooltip("The maximum distance between the bottom of this character and the ground where the character will " +
                 "still be considered grounded. This is useful for preventing the player from being considered " +
                 "ungrounded when moving over small bumps or crevices in the ground or when slightly launching into " +
                 "the air when moving over an abrupt slope change.")]
        [Min(0)] public float padding = .1f;
        [Tooltip("The distance out over a ledge the center of the character can rest while still being considered " +
                 "grounded. This is useful for preventing the character from being able to stand still on the edge " +
                 "of a ledge causing the character mesh to look like its floating in the air.")]
        [Min(0)] public float overhangTolerance = .15f;
        [Tooltip("Optional. The IFreeFall component on this GameObject if any.")]
        public InterfaceRef<IFreeFall> freeFall;
        [Tooltip("A list of specific colliders that should not be considered ground. This can be useful for " +
                 "preventing other child colliders under this character or any specific colliders in the " +
                 "scene from being considered ground without modifying the layers their GameObjects are on.")]
        [SerializeField] List<Collider> ignoredColliders = new();
        readonly RaycastHit[] hits = new RaycastHit[16];
        readonly List<float> contactDegrees = new(); // pre-allocated list
        readonly List<ContactPoint> contacts = new();
        Coroutine lateFixedUpdateCoroutine;
        bool _isGrounded;
        uint contactsPhysicsFrame;
        uint physicsFrame;
        bool rawIsOnSteepSlope;
        bool canFeetTouchGround;
        bool isSurroundedByContacts;
        float steepSlopeValue;
        [SerializeField, Range(0, 1)] float freeFallValue;
        Vector3 priorVelocity;
        Offset3 _localVelocityGoal;
        Vector3? localStillPosition;
        Capsule? contactCapsule;
        Capsule currentCapsule;
        Vector3 groundFeetForce;
        Vector3 rawGroundFeetForce;

        const float tolerance = .01f;
        const float freeFallValueSpeed = 2;
        const float steepSlopeValueRate = 5;
        const float groundFeetForceRate = 10000;

        /// <summary>
        /// Object Entity IDs for specific colliders that should not be considered ground.
        /// </summary>
        public HashSet<EntityId> IgnoredColliderIds { get; } = new();

        public bool IsGrounded { get; set; }
        public bool IsOnSteepSlope { get; private set; }
        public float GroundDistance { get; private set; }
        public Surface Ground { get; private set; } = new();
        public Vector3 MoveInput { get; set; }
        /// <summary>
        /// The current velocity of this character relative to the ground point velocity.
        /// </summary>
        public Vector3 LocalVelocity { get; private set; }
        public Vector3 LocalVelocityGoal
        {
            get => _localVelocityGoal.Value;
            set => _localVelocityGoal = new Offset3(value);
        }
        public float MaxLocalSpeed { get; set; } = 10;
        public Vector3 Acceleration { get; private set; }
        public float AccelerationGoal { get; set; } = 30;
        public float MaxAcceleration { get; set; } = 30;
        public Rigidbody Rigidbody { get; private set; }
        public event Action<ICharacterMotor> Moving;
        /// <summary>
        /// The CapsuleCollider attached to this GameObject.
        /// </summary>
        public CapsuleCollider CapsuleCollider { get; private set; }
        /// <summary>
        /// The current world space center of this character's CapsuleCollider.
        /// </summary>
        public Vector3 Center => currentCapsule.Center;


        void Reset()
        {
            OnValidate();
            foreach (Collider childCollider in GetComponentsInChildren<Collider>())
            {
                ignoredColliders.Add(childCollider);
            }
        }

        void OnValidate()
        {
            freeFall.Value ??= GetComponent<IFreeFall>();
            CapsuleCollider = GetComponent<CapsuleCollider>();
            overhangTolerance = Mathf.Clamp(overhangTolerance, 0, CapsuleCollider.radius);
            steepSlopeThresholdDegrees = Mathf.Clamp(steepSlopeThresholdDegrees, 0, 90);
        }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            CapsuleCollider = GetComponent<CapsuleCollider>();
            layerMask |= ~GetMaskWithAllDefinedLayers();
            foreach (Collider groundingIgnoredCollider in ignoredColliders)
            {
                IgnoredColliderIds.Add(groundingIgnoredCollider.GetEntityId());
            }
            if (!CapsuleCollider.sharedMaterial)
            {
                CapsuleCollider.material = CreateZeroFrictionZeroBounceMaterial();
            }
        }

        void OnEnable()
        {
            lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
        }

        void OnDisable()
        {
            IsGrounded = false;
            IsOnSteepSlope = false;
            GroundDistance = float.PositiveInfinity;
            Ground.Clear();
            if (lateFixedUpdateCoroutine != null) StopCoroutine(lateFixedUpdateCoroutine);
        }

        void FixedUpdate()
        {
            ClearOldContacts();
            physicsFrame++;
            float dt = Time.deltaTime;
            if (dt <= 0) return;

            overhangTolerance = Mathf.Clamp(overhangTolerance, 0, CapsuleCollider.radius);
            steepSlopeThresholdDegrees = Mathf.Clamp(steepSlopeThresholdDegrees, 0, 90);
            currentCapsule = new Capsule(CapsuleCollider, Rigidbody.position + CapsuleCollider.center);
            UpdateAcceleration();
            UpdateGrounding(GetForwardGroundContact()); // uses contacts
            contacts.Clear();
            _localVelocityGoal.Direction = SnapPointToPlaneVertically(_localVelocityGoal.Direction, Ground.Normal);
            Moving?.Invoke(this);
            _localVelocityGoal.Direction = SnapPointToPlaneVertically(_localVelocityGoal.Direction, Ground.Normal);
            Move();
            contactCapsule = null;
        }

        // internal Unity physics update after FixedUpdate but before OnCollision

        void OnCollisionEnter(Collision other)
        {
            ClearOldContacts();
            contactCapsule = new Capsule(CapsuleCollider);
            for (int i = 0; i < other.contactCount; i++)
            {
                contacts.Add(other.GetContact(i));
            }
        }

        void OnCollisionStay(Collision other)
        {
            ClearOldContacts();
            contactCapsule = new Capsule(CapsuleCollider);
            for (int i = 0; i < other.contactCount; i++)
            {
                contacts.Add(other.GetContact(i));
            }
        }

        IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitForFixedUpdate = new();
            while (enabled)
            {
                yield return waitForFixedUpdate;
                StaticFrictionSnap(); // uses contacts
            }
        }

        void ClearOldContacts()
        {
            if (contactsPhysicsFrame != physicsFrame)
            {
                contacts.Clear();
                contactsPhysicsFrame = physicsFrame;
            }
        }

        void UpdateAcceleration()
        {
            Vector3 velocity = Rigidbody.linearVelocity;
            Acceleration = (velocity - priorVelocity) / Time.deltaTime;
            priorVelocity = velocity;
        }

        ContactPoint? GetForwardGroundContact()
        {
            Vector3 horizontalVelocityGoal = LocalVelocityGoal == default ? Vector3.down : LocalVelocityGoal.WithY(0);
            float maxContactForwardDot = float.MinValue;
            Capsule capsule = contactCapsule ?? currentCapsule;
            ContactPoint? goalContact = null;
            foreach (ContactPoint contact in contacts)
            {
                Collider groundCollider = ContactCollider(contact, CapsuleCollider);
                if (!groundCollider) continue;
                if (contact.separation > tolerance) continue;
                float contactSlopeDegrees = Vector3.Angle(Vector3.up, ContactNormal(contact, capsule.GetLowerCenter()));
                if (contactSlopeDegrees > 89) continue;
                int layer = groundCollider.gameObject.layer;
                if (((1 << layer) & layerMask) == 0) continue;
                if (contactSlopeDegrees > steepSlopeThresholdDegrees) continue;
                float dot = Vector3.Dot(horizontalVelocityGoal, contact.point);
                if (dot > maxContactForwardDot)
                {
                    maxContactForwardDot = dot;
                    goalContact = contact;
                }
            }
            return goalContact;
        }

        void UpdateGrounding(ContactPoint? groundContact)
        {
            Capsule capsule = contactCapsule ?? currentCapsule;
            if (groundContact.HasValue)
            {
                GroundDistance = 0;
                Ground.Update(
                    groundContact.Value.point,
                    ContactNormal(groundContact.Value, capsule.GetLowerCenter()),
                    ContactCollider(groundContact.Value, CapsuleCollider),
                    1,
                    .6f);
            }
            else
            {
                Sphere castSphere = new Sphere(capsule.GetLowerCenter(), capsule.radius - tolerance);
                RaycastHit groundHit = Spherecast(castSphere, Vector3.down, hits, float.PositiveInfinity, layerMask, IgnoredColliderIds, QueryTriggerInteraction.Ignore);
                if (groundHit.IsValid())
                {
                    GroundDistance = groundHit.distance - tolerance;
                    Ground.Update(
                        groundHit.point,
                        groundHit.normal,
                        groundHit.collider,
                        1,
                        .6f);
                }
                else
                {
                    GroundDistance = float.PositiveInfinity;
                    Ground.Clear();
                }
            }
            canFeetTouchGround = CanFeetTouchGround(capsule);
            isSurroundedByContacts = IsSurroundedByContacts();
            bool isFreeFalling = IsFreeFalling(Rigidbody, Acceleration, freeFall.Value);
            freeFallValue = Mathf.MoveTowards(freeFallValue, isFreeFalling ? 1 : 0, freeFallValueSpeed * Time.deltaTime);
            bool isFreeFallingContinually = freeFallValue > .99f;
            IsGrounded = !isFreeFallingContinually && (GroundDistance <= padding & canFeetTouchGround) | isSurroundedByContacts;
            rawIsOnSteepSlope = Ground.SlopeDegrees > steepSlopeThresholdDegrees;
            float steepSlopeValueGoal = IsGrounded & !isSurroundedByContacts & (rawIsOnSteepSlope | !canFeetTouchGround) ? 1 : 0;
            steepSlopeValue = Mathf.MoveTowards(steepSlopeValue, steepSlopeValueGoal, steepSlopeValueRate * Time.deltaTime);
            IsOnSteepSlope = steepSlopeValue > 1 - tolerance;
        }

        bool CanFeetTouchGround(in Capsule capsule)
        {
            float radius = capsule.radius;
            float localFeetContactHeight = Ground.Point.y - capsule.GetLowerTip().y;
            float localFeetContactMaxHeight = radius - Mathf.Sqrt(radius * radius - overhangTolerance * overhangTolerance);
            if (localFeetContactHeight < localFeetContactMaxHeight) return true;

            float flatness = Mathf.Cos(steepSlopeThresholdDegrees * Mathf.Deg2Rad);
            flatness = Mathf.Max(flatness, .001f);
            float feetCastDistance = radius / flatness - overhangTolerance / flatness + tolerance;
            if (feetCastDistance > capsule.GetLength() * 5) return true;

            Sphere feetSphere = new(capsule.GetLowerCenter(), overhangTolerance);
            RaycastHit feetHit = Spherecast(
                feetSphere,
                Vector3.down,
                hits,
                feetCastDistance,
                layerMask,
                IgnoredColliderIds);
            return feetHit.IsValid();
        }

        bool IsSurroundedByContacts()
        {
            contactDegrees.Clear();
            Capsule capsule = contactCapsule ?? currentCapsule;
            for (int i = 0; i < contacts.Count; i++)
            {
                Vector3 normal = contacts[i].normal;
                if (contacts[i].point.y > capsule.GetLowerCenter().y) continue;
                if (Mathf.Abs(normal.x) < tolerance & Mathf.Abs(normal.z) < tolerance) continue;
                float degrees = Mathf.Atan2(-normal.z, -normal.x) * Mathf.Rad2Deg;
                if (degrees < 0) degrees += 360;
                contactDegrees.Add(degrees);
            }
            if (contactDegrees.Count <= 1) return false;
            Sort(contactDegrees, Comparer<float>.Default);
            float maxDegreesDiff = 0;
            for (int i = 0; i < contactDegrees.Count; i++)
            {
                int j = (i + 1) % contactDegrees.Count;
                float degreesI = contactDegrees[i];
                float degreesJ = contactDegrees[j];
                if (j == 0) degreesJ += 360;
                float degreesDiff = degreesJ - degreesI;
                if (maxDegreesDiff < degreesDiff)
                {
                    maxDegreesDiff = degreesDiff;
                }
            }
            bool isSurrounded = maxDegreesDiff < 181;
            return isSurrounded;
        }

        void Move()
        {
            LocalVelocity = Rigidbody.linearVelocity - Ground.PointSecantVelocity;
            if (!IsGrounded) return;
            Vector3 localVel = LocalVelocity;
            localStillPosition = null;

            if (IsGrounded)
            {
                // steep slope friction
                Offset3 slopeLocalVelocity = new(Rejection(LocalVelocity, Ground.Normal));
                if (slopeLocalVelocity.IsValid())
                {
                    float currentGravity = GetGravity(Rigidbody, freeFall.Value).y;
                    float frictionMagnitude = FrictionAccelerationMagnitude(Ground.SlopeDegrees, currentGravity, Ground.DynamicFriction);
                    Offset3 steepSlopeFriction = new(-slopeLocalVelocity.Direction * frictionMagnitude);
                    localVel += steepSlopeFriction.Value * Time.deltaTime * steepSlopeValue;
                }

                // apply move input
                float frictionMultiplier = localVel.magnitude < MaxLocalSpeed ? Ground.StaticFriction : Ground.DynamicFriction;
                Vector3 moveDelta = MoveDelta(localVel, LocalVelocityGoal, AccelerationGoal * frictionMultiplier * Time.deltaTime);
                moveDelta.y = Mathf.Max(moveDelta.y, 0);
                localVel += moveDelta * (1 - steepSlopeValue);

                // feet ground force
                Vector3 feetForce = (localVel - LocalVelocity) / Time.deltaTime * Rigidbody.mass;
                rawGroundFeetForce = -feetForce;
                // instantly applying the full feet force can lead to a jittery feedback loop as the character moves the ground object then tries to accelerate to match its point velocity
                groundFeetForce = Vector3.MoveTowards(groundFeetForce, -feetForce, groundFeetForceRate * Time.deltaTime);
                if (Ground.Rigidbody && !Ground.Rigidbody.isKinematic)
                {
                    float maxGroundForce = Mathf.Min(groundFeetForce.magnitude, feetForce.magnitude);
                    Ground.Rigidbody.AddForceAtPosition(Vector3.ClampMagnitude(groundFeetForce, maxGroundForce), Ground.Point, ForceMode.Force);
                }

                // record isStill
                if (GroundDistance < tolerance
                    && !IsOnSteepSlope
                    && localVel is { x: 0, z: 0 }
                    && _localVelocityGoal.Magnitude == 0)
                {
                    localStillPosition = Ground.Rigidbody
                        ? Ground.Rigidbody.WorldToLocalPoint(Rigidbody.position)
                        : Rigidbody.position;
                }
            }
            Rigidbody.SetVelocity(localVel + Ground.PointSecantVelocity);
            Rigidbody.AddForce(default, ForceMode.VelocityChange);
        }

        void StaticFrictionSnap()
        {
            if (!IsGrounded || !localStillPosition.HasValue) return;
            Capsule capsule = contactCapsule ?? currentCapsule;
            Vector3 localVel = Rigidbody.linearVelocity - Ground.PointSecantVelocity;
            float staticFrictionAcc = FrictionAccelerationMagnitude(
                Ground.SlopeDegrees,
                MaxAcceleration,
                Ground.StaticFriction);
            float maxVelocityDelta = staticFrictionAcc * Time.fixedDeltaTime;
            bool isSlowSlide = localVel.magnitude < maxVelocityDelta;
            bool isMovingUp = localVel.y > tolerance;
            if (isSlowSlide & !isMovingUp)
            {
                foreach (ContactPoint contact in contacts)
                {
                    bool needsToMove = contact.separation < -tolerance;
                    if (needsToMove) return;
                }
                Vector3 worldStillPosition = Ground.Rigidbody
                    ? Ground.Rigidbody.LocalToWorldPoint(localStillPosition.Value)
                    : localStillPosition.Value;
                Offset3 offset = new(worldStillPosition - Rigidbody.position);
                RaycastHit hit = Capsulecast(
                    capsule.WithRadius(capsule.radius - tolerance),
                    offset.Direction,
                    hits,
                    offset.Magnitude + tolerance,
                    layerMask,
                    IgnoredColliderIds);
                if (hit.IsValid()) return;
                Rigidbody.MovePosition(worldStillPosition);
                Rigidbody.SetVelocity(Ground.PointSecantVelocity);
            }
        }
    }
}