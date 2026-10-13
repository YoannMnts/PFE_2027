using PFE.Core.Scripts.Enemy.Dummy;
using PFE.Debugging._Project.Scripts.Debugging;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.DummyEnemy.Runtime
{
    public class RuntimeEnemyTemplate : RuntimeEnemy
    {
        // Dummy (MakeNpcDummy on the prefab): goes back to its spawn point instead of chasing the pilgrim
        private bool isDummy;
        private Vector3 startPosition;

        protected override void Awake()
        {
            base.Awake();
            isDummy = TryGetComponent<MakeNpcDummy>(out _);
        }

        // Called once the NPC is placed on its spawn anchor. Awake is too early: it runs during the
        // instantiation, while the NPC is still at the prefab position.
        protected override void OnSetup()
        {
            base.OnSetup();

            if (!isDummy)
                return;

            startPosition = transform.position;
        }

        protected override void UpdateTargetPosition(Vector3 position)
        {
            base.UpdateTargetPosition(isDummy ? startPosition : position);
        }
    }
}
