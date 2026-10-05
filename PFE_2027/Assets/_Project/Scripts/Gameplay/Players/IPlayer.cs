using Helteix.ChanneledProperties.Priorities;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players
{
    public interface IPlayer
    {
        public Priority<bool> ShowUI { get; }
    }
}
