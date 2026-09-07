using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Applies extra gravity and drag forces to the attached Rigidbody and checks if it is
    /// currently free falling.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Character Physics/Free Fall")]
    [RequireComponent(typeof(Rigidbody))]
    public class FreeFall : MonoBehaviour, IFreeFall
    {
        [Tooltip("The speed during free fall where gravity and air resistance balance each other " +
                 "and the body stops accelerating. It is generally the max speed of a body during " +
                 "free fall if no other forces are applied to it. It is recommended to set Rigidbody.drag " +
                 "(Rigidbody.linearDamping in code) to 0 so extra drag forces aren't applied. If you do " +
                 "not want drag forces applied by this component, you can set this value to a high number such as " +
                 "one million.")]
        [SerializeField, Min(0)] float _terminalSpeed = 53;
        [Tooltip("How much gravity relative to the project's default gravity is applied to the attached " +
                 "Rigidbody. You can change the project's gravity in the following menu: " +
                 "Edit/Project Settings/Physics/Settings/Shared/Gravity")]
        [SerializeField] float _gravityScale = 1;
        Offset3 velocity;

        const float gravityPercentThreshold = .9f;
        
        public Vector3 Drag { get; private set; }
        public bool IsFreeFalling { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        
        public float TerminalSpeed
        {
            get => _terminalSpeed;
            set => _terminalSpeed = Mathf.Max(value, 0);
        }
        public float GravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }
        public Vector3 Gravity { get; private set; }

        void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (!Rigidbody.useGravity) return;
            
            float dt = Time.deltaTime;
            if (dt <= 0) return;
            
            Vector3 projectGravity = Physics.gravity;
            Gravity = projectGravity * GravityScale;

            Vector3 priorVelocity = velocity.Value;
            velocity = new Offset3(Rigidbody.linearVelocity);
            Drag = -velocity.Direction * DragAcceleration(velocity.Magnitude, TerminalSpeed, Gravity.y);
            
            Vector3 accelerationExpected = Gravity + Drag;
            Vector3 accelerationActual = (velocity.Value - priorVelocity) / dt;
            float accelerationDiff = (accelerationActual - accelerationExpected).magnitude;
            IsFreeFalling = accelerationDiff < Gravity.magnitude * (1 - gravityPercentThreshold);
            
            Vector3 gravityBonus = projectGravity * (GravityScale - 1);
            Rigidbody.AddForce(Drag + gravityBonus, ForceMode.Acceleration);
        }
    }
}