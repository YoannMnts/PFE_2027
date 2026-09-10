using PFE.Core.Scripts.AIPattern;
using UnityEngine;

namespace PFE.Core
{
    public abstract class Boss : IBoss<BossData>
    {
        public void Spawn(BossData data)
        {
        }

        public void Attack(BossData data)
        {
        }
    }
}
