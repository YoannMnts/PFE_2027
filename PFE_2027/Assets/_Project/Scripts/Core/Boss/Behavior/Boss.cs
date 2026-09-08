using PFE.Core.Scripts.AIPattern;
using UnityEngine;

namespace PFE.Core
{
    public abstract class Boss : IBoss<BossData>
    {
        public void Spawn(BossData data)
        {
        }

        public void Move(BossData data, MovePattern movePattern)
        {
        }

        public void Attack(BossData data, AttackPattern attackPattern)
        {
        }
    }
}
