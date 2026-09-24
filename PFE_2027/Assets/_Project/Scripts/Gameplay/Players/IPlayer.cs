using Helteix.ChanneledProperties.Priorities;

namespace PFE.Gameplay.Scripts.Players
{
    public interface IPlayer
    {
        public Priority<bool> ShowUI { get; }
    }
}
