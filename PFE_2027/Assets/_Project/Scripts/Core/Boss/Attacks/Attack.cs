namespace PFE.Core.Scripts.Attacks
{
    public abstract class Attack : IAttack<AttackData>
    {
        public void CanAttack(AttackData data)
        {
        }

        public void Execute(AttackData data)
        {
        }
    }
}