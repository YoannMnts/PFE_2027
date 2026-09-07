using Unity.Cinemachine;
using UnityEngine;
using static TraversalPro.Utility;

namespace TraversalPro
{
    /// <summary>
    /// Smoothly modifies the field-of-view of an attached Cinemachine camera when the character starts or stops sprinting.
    /// </summary>
    [AddComponentMenu("Traversal Pro/Camera/Cinemachine Sprint Effect")]
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineSprintEffect : MonoBehaviour
    {
        [Tooltip("The Character Run component to read to know if this character is sprinting.")]
        public CharacterRun characterRun;
        [Tooltip("The ICharacterMotor component to read to know if this character is grounded.")]
        public InterfaceRef<ICharacterMotor> characterMotor;
        [Tooltip("The bonus field-of-view in degrees to add to the camera's base field-of-view when sprinting.")]
        public float sprintFOVBonus = 10;
        [Tooltip("Approximately how long in seconds it will take for the camera's field-of-view to " +
                 "smoothly reach the goal field-of-view when starting or stopping sprinting.")]
        public float smoothTime = .25f;
        CinemachineCamera runCam;
        /// <summary>
        /// The base field-of-view of the camera when not sprinting. This value is automatically copied from the
        /// attached Cinemachine camera during Start but can later be modified if needed.
        /// </summary>
        public float BaseFOV { get; private set; }
        float velocity;
        bool wasSprintEffect;

        void Reset()
        {
            OnValidate();
        }

        void OnValidate()
        {
            if (!characterRun) characterRun = transform.root.GetComponentInChildren<CharacterRun>();
            characterMotor.Value ??= transform.root.GetComponentInChildren<ICharacterMotor>();
        }

        void Awake()
        {
            runCam = GetComponent<CinemachineCamera>();
        }

        void OnEnable()
        {
            if (!TryValidateRequiredField(this, characterMotor.Value)
                || !TryValidateRequiredField(this, characterRun))
            {
                enabled = false;
            }
            BaseFOV = runCam.Lens.FieldOfView;
        }
        
        void LateUpdate()
        {
            if (Time.deltaTime <= 0) return;
            bool isSprintEffect = characterRun.IsSprinting && (characterMotor.Value.IsGrounded || wasSprintEffect);
            float fovGoal = isSprintEffect ? BaseFOV + sprintFOVBonus : BaseFOV;
            runCam.Lens.FieldOfView = Mathf.SmoothDamp(runCam.Lens.FieldOfView, fovGoal, ref velocity, smoothTime);
            wasSprintEffect = isSprintEffect;
        }
    }
}
