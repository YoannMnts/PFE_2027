using PFE.Core.Scripts.ECS;
using PFE.Gameplay.Scripts.CrossRoadGameModes;

namespace PFE.Gameplay.Scripts.Entities
{
    public interface IRuntimeEntity
    {
        public EntityAddress Address { get; }
        public void SetActive();
        void Connect(CrossRoadGameModeContext context, EntityAddress address);
        void Disconnect();
    }
}