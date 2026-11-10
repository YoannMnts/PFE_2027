using PFE.Core.Scripts.ECS;
using PFE.Gameplay.Scripts.CrossRoadGameModes;

namespace PFE.Gameplay.Scripts.Entities
{
    public interface IRuntimeEntity
    {
        public EntityAddress Address { get; }
        void Connect(CrossRoadGameModeContext context, EntityAddress address);
        void Disconnect();
    }
}