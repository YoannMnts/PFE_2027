using Helteix.ChanneledProperties.Priorities;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.Players
{
    public interface IPlayer
    {
        public ComponentGroup ComponentGroup { get;  }
        public Priority<bool> ShowUI { get; }

        public void SetupComponentGroup(ComponentGroupData data);
    }
}