using UnityEngine;

namespace TraversalPro
{
    public class CursorLockHide : MonoBehaviour
    {
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}