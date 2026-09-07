using UnityEngine;

namespace TraversalPro
{
    /// <summary>
    /// Applies extra gravity and drag forces to the attached Rigidbody and checks if the it is currently free falling.
    /// </summary>
    public interface IFreeFall
    {
        /// <summary>
        /// The speed during free fall where gravity and air resistance balance each other and the 
        /// Rigidbody stops accelerating. It is generally the max speed of a body during free fall if no
        /// other forces are applied to it. It is recommended to set Rigidbody.drag (Rigidbody.linearDamping
        /// in code) to 0 so extra drag forces aren't applied. If you do not want drag forces applied by this
        /// component, you can set this value to a high number such as one million.
        /// </summary>
        public float TerminalSpeed { get; set; }
        /// <summary>
        /// How much gravity relative to the project's default gravity is applied to the Rigidbody. 
        /// You can change the project's gravity in the following menu: 
        /// Edit/Project Settings/Physics/Settings/Shared/Gravity
        /// </summary>
        public float GravityScale { get; set; }
        /// <summary>
        /// The current Gravity acceleration being applied to the Rigidbody.
        /// </summary>
        public Vector3 Gravity { get; }
        /// <summary>
        /// The current Drag acceleration being applied to the Rigidbody.
        /// </summary>
        public Vector3 Drag { get; }
        /// <summary>
        /// Is the Rigidbody currently free falling?
        /// </summary>
        public bool IsFreeFalling { get; }
        /// <summary>
        /// The attached Rigidbody that accelerations will be applied to.
        /// </summary>
        public Rigidbody Rigidbody { get; }
    }
}