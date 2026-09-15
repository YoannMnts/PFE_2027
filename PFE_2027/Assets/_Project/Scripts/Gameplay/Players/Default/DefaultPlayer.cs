using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using PFE.Core.Scripts.ComponentSystem;
using PFE.Gameplay.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.Players.Default
{
    public class DefaultPlayer : IPlayer
    {
        public ComponentGroup ComponentGroup { get; private set; }
        
        public Priority<bool> ShowUI { get; private set; }

        public DefaultPlayer()
        {
            ShowUI = new Priority<bool>(false);
        }

        public void SetupComponentGroup(ComponentGroupData data)
        {
            ComponentGroup = new ComponentGroup(data);
        }
    }
}
