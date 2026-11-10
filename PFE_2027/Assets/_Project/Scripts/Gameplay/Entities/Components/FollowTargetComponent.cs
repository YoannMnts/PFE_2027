using PFE.Core.Scripts.ECS;

namespace PFE.Gameplay.Scripts.Components
{
    public struct FollowTargetComponent
    {
        public Entity Target { get; private set; }

        public FollowTargetComponent(Entity target)
        {
            Target = target;
        }
        
        public void SetTarget(Entity target) => Target = target;
    }
}