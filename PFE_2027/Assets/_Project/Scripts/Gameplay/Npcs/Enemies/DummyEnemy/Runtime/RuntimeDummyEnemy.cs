using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    // Training dummy: always goes back to its spawn point, whatever the pilgrim does
    public class RuntimeDummyEnemy : RuntimeEnemy
    {
        // Setup runs while the NPC stands on its anchor (before it is hidden): that's its home
        protected override void OnSetup(CrossRoadGameModeContext context)
        {
            base.OnSetup(context);
            Instance.SetGoal(transform.position);
        }

        // Ignores the pilgrim: its goal never changes
        protected override void OnPilgrimMoved(Vector3 pilgrimPosition) { }
    }
}
