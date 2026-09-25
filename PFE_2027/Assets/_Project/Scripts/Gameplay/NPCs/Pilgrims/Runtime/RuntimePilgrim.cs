using UnityEngine;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : MonoBehaviour
    {
        private Pilgrim pilgrim;

        public void Setup(Pilgrim currentPilgrim )
        {
            pilgrim = currentPilgrim;
        }

        public void FixedUpdate()
        {
            if (pilgrim != null)
            {
                pilgrim.UpdatePosition(transform.position);
            }
        }
    }
}