using UnityEngine;

namespace TraversalPro
{
    /// <summary>
    /// Applies instantaneous upward acceleration to a Rigidbody to simulate jumping.
    /// </summary>
    public interface IJump
    {
        /// <summary>
        /// The last time a jump was performed.
        /// </summary>
        public double LastJumpTime { get; }
        /// <summary>
        /// Request a jump. Various jump rules will be checked to determine if a jump is allowed. The jump
        /// may occur on a later frame.
        /// </summary>
        public void RequestJump();
        /// <summary>
        /// The Rigidbody the jump forces will be applied to.
        /// </summary>
        public Rigidbody Rigidbody { get; }
    }
}