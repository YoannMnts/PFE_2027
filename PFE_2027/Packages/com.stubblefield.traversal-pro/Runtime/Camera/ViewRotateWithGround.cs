using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Rotates its Transform along the y axis at the same angular velocity as the ground its character is standing on.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Camera/View Rotate With Ground")]
    [RequireComponent(typeof(ViewControl))]
    public class ViewRotateWithGround : MonoBehaviour
    {
        [Tooltip("An IGrounding component to read ground values from.")]
        public InterfaceRef<ICharacterMotor> characterMotor;
        [Tooltip("When free falling above rotating ground, what is the max distance the ground can be for " +
                 "the camera to rotate with the ground?")]
        [Min(0)] public float maxGroundDistance = 1;
        [Tooltip("A ground rigidbody's mass must be this many times larger than this character's mass for it to cause " +
                 "the character's view to rotate.")]
        [Min(0)] public float groundMassFactorThreshold = 2;
        [Tooltip("The approximate duration in seconds it should take for this transform's rotation speed to match the " +
                 "ground's rotation speed. Smaller values result in greater accelerations.")]
        [Min(0)] public float smoothTime = .05f;
        ViewControl viewControl;
        float velocity;
        float acceleration;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            characterMotor.Value ??= GetComponentInParent<ICharacterMotor>();
        }

        void Awake()
        {
            viewControl = GetComponent<ViewControl>();
        }

        void OnEnable()
        {
            if (!TryValidateRequiredField(this, characterMotor.Value)) enabled = false;
            velocity = default;
            acceleration = default;
        }

        void Update()
        {
            float speedGoal = characterMotor.Value.Ground.AngularVelocity.y * Mathf.Rad2Deg;
            if (characterMotor.Value.GroundDistance > maxGroundDistance)
            {
                speedGoal = 0;
            }
            if (characterMotor.Value.Ground.Rigidbody 
                && !characterMotor.Value.Ground.Rigidbody.isKinematic
                && characterMotor.Value.Ground.Rigidbody.mass < characterMotor.Value.Rigidbody.mass * groundMassFactorThreshold)
            {
                speedGoal = 0;
            }
            velocity = Mathf.SmoothDamp(velocity, speedGoal, ref acceleration, smoothTime);
            viewControl.DegreesGoal += new Vector3(0, velocity * Time.deltaTime, 0);
        }
    }
}