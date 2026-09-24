using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default
{
    public class DefaultPlayer : IPlayer, IDisposable
    {
        public Priority<bool> ShowUI { get; private set; }

        public DefaultPlayer()
        {
            ShowUI = new Priority<bool>(false);
        }

        public void Dispose()
        {
        }
    }
}
