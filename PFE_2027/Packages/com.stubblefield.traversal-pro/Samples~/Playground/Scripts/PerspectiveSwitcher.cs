using Unity.Cinemachine;
using UnityEngine;

namespace TraversalPro.Samples.Playground
{
    public class PerspectiveSwitcher : MonoBehaviour
    {
        [SerializeField] CinemachineCamera firstPersonCamera;
        [SerializeField] CinemachineCamera thirdPersonCamera;
        [SerializeField] GameObject thirdPersonMesh;
        [SerializeField] Jump jump;
        [SerializeField] float thirdPersonJumpDelay = .1f;
        [SerializeField] bool isFirstPerson;
        bool isDirty = true;

        void Update()
        {
            if (Time.deltaTime <= 0) return;
            if (isDirty)
            {
                UpdatePerspective();
                isDirty = false;
            }
        }

        public void SwitchPerspective()
        {
            isFirstPerson = !isFirstPerson;
            isDirty = true;
        }

        void UpdatePerspective()
        {
            if (isFirstPerson)
            {
                firstPersonCamera.Priority = 1;
                thirdPersonCamera.Priority = -1;
                if (thirdPersonMesh) thirdPersonMesh.SetActive(false);
                jump.delay = 0;
            }
            else
            {
                firstPersonCamera.Priority = -1;
                thirdPersonCamera.Priority = 1;
                if (thirdPersonMesh) thirdPersonMesh.SetActive(true);
                jump.delay = thirdPersonJumpDelay;
            }
        }
    }
}