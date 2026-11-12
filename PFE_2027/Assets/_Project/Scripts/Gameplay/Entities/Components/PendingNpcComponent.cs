using PFE.Core.Scripts.ECS;

namespace PFE.Gameplay.Scripts.Components
{
    public readonly struct PendingNpcComponent : IEntityComponent
    {
        public readonly float progressionGoal;

        public PendingNpcComponent(float progressionGoal)
        {
            this.progressionGoal = progressionGoal;
        }
    }
}