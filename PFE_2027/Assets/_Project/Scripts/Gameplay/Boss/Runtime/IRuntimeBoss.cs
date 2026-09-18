using System;
using PFE.Core;

namespace PFE.Gameplay.Scripts.Phases
{
    public interface IRuntimeBoss
    {
        public void Setup(BossInstance bossInstance);
    }
}