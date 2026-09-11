using PFE.Core.Scripts.AIPattern;
using PFE.Core.Scripts.Attacks;
using UnityEngine;

namespace PFE.Core
{
    public abstract class Boss : IBoss<BossData>
    {
        public bool CanSpawn(BossData data)
        {
            //if Attack => false
            //data.StageBalances[0].AttitudeData.
             return true;
        }

        public void Attack(BossData data)
        {
            
        }
    }
}
