using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Enemy.Templates;

namespace PFE.Gameplay.Scripts.Enemy.Templates
{
    public class AttackTemplate : Attack<AttackDataTemplate>
    {
        public override bool CanDoAttack(AttackDataTemplate data)
        {
            return base.CanDoAttack(data);
        }

        protected override void Trigger()
        {
            //logic de l'attaque spécifique
        }
    }
}