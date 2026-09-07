using System;
using UnityEngine;

namespace TraversalPro
{
    /// <summary>
    /// Detects ground and applies forces to the attached Rigidbody to slide it along the ground.
    /// This is meant to simulate a character moving along the ground.
    /// </summary>
    public interface ICharacterMotor
    {
        /// <summary>
        /// Is this character currently grounded?
        /// </summary>
        public bool IsGrounded { get; set; }
        /// <summary>
        /// Is this character currently on a steep slope?
        /// </summary>
        public bool IsOnSteepSlope { get; }
        /// <summary>
        /// The distance between the bottom of this character and the ground surface below it.
        /// </summary>
        public float GroundDistance { get; }
        /// <summary>
        /// Info about the current ground surface.
        /// </summary>
        public Surface Ground { get; }
        /// <summary>
        /// The movement input vector for this character. Its magnitude should be in the range [0,1]. The Y component
        /// should generally be 0.
        /// </summary>
        public Vector3 MoveInput { get; set; }
        /// <summary>
        /// The current velocity of this character relative to the current ground point velocity.
        /// </summary>
        public Vector3 LocalVelocity { get; }
        /// <summary>
        /// The movement velocity goal of this character relative to the current ground point velocity. 
        /// </summary>
        public Vector3 LocalVelocityGoal { get; set; }
        /// <summary>
        /// The maximum movement speed of this character relative to the current ground point velocity.
        /// The purpose of this value is to define the expected range for the local velocity's magnitude.
        /// </summary>
        public float MaxLocalSpeed { get; set; }
        /// <summary>
        /// The current acceleration of the Rigidbody attached to this character.
        /// </summary>
        public Vector3 Acceleration { get; }
        /// <summary>
        /// The acceleration at which the character's velocity will move towards the movement velocity goal.
        /// </summary>
        public float AccelerationGoal { get; set; }
        /// <summary>
        /// The maximum movement acceleration this character might apply to reach the goal velocity.
        /// The purpose of this value is to define the expected range for the acceleration's magnitude.
        /// </summary>
        public float MaxAcceleration { get; set; }
        /// <summary>
        /// The Rigidbody that character movement forces are applied to.
        /// </summary>
        public Rigidbody Rigidbody { get; }
        /// <summary>
        /// This event is called each Fixed Update after ground detection but before movement forces are applied.
        /// This can be used, for example, to modify grounding before movement or modify movement in response to
        /// ground detection.
        /// </summary>
        public event Action<ICharacterMotor> Moving;
    }
}