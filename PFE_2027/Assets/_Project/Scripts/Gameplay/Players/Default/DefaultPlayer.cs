using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;

namespace PFE.Gameplay.Scripts.Players.Default
{
    public class DefaultPlayer : IPlayer, IDisposable
    {
        public Priority<bool> ShowUI { get; private set; }

        public DefaultPlayer()
        {
            ShowUI = new Priority<bool>(false);
        }

        public void CastAttack()
        {
            // TODO: systeme de component supprime, a reimplementer avec le nouveau systeme de combat
            // ComponentGroup.TriggerAllComponents(this, OnComponentTrigger);
        }

        public void Dispose()
        {
        }
    }
}
