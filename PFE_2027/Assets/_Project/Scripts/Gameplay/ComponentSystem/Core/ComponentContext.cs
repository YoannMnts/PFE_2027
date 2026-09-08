namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public struct ComponentContext
    {
        public readonly DurationController durationController;

        public ComponentContext(DurationController durationController)
        {
            this.durationController = durationController;
        }
    }
}