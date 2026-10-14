using PFE.Gameplay.Scripts.Enemy.Runtime;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public class RuntimeDummyEnemy : RuntimeEnemy
    {
        private Vector3 startPosition;

        protected override void OnSetup()
        {
            base.OnSetup();
            startPosition = transform.position;
        }

        protected override void OnPilgrimMoved(Vector3 pilgrimPosition)
        {
            base.OnPilgrimMoved(startPosition);
        }
    }
}