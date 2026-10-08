using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.NPCs
{
    public struct NpcInstanceContext
    {
        public readonly SplineContainer splineContainer;

        public NpcInstanceContext(SplineContainer splineContainer)
        {
            this.splineContainer = splineContainer;
        }
    }
}