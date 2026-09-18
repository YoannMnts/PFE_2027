using PFE.Gameplay.Scripts.Players;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public struct ComponentContext
    {
        public readonly DurationController durationController;
        public readonly IPlayer player;

        public ComponentContext(DurationController durationController, IPlayer player)
        {
            this.durationController = durationController;
            this.player = player;
        }
    }
}