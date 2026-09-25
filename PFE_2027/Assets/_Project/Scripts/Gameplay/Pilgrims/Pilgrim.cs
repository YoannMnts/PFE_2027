using UnityEngine;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class Pilgrim
    {
        private Vector3 currentPosition;

        public void UpdatePosition(Vector3 newPosition)
        {
            currentPosition = newPosition;
        }
        
    }
}