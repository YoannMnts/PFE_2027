using PFE.Core.Scripts.Templates;

namespace PFE.Core.Scripts.Attacks
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