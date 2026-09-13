using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.Players
{
    public interface IPlayer
    {
        public ComponentGroup ComponentGroup { get;  }

        public void SetupComponentGroup(ComponentGroupData data);
    }
}