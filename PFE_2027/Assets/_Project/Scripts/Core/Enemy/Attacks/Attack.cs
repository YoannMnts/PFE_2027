namespace PFE.Core.Scripts.Attacks
{
    public abstract class Attack<TData> : IAttack<TData> where TData : AttackData
    {
        public virtual bool CanDoAttack(TData data) => true;

        public void Execute(TData data, int damage)
        {
            if (!CanDoAttack(data))
                return;
            Trigger();
        }

        protected abstract void Trigger();
    }
}