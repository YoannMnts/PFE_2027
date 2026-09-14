using PFE.Core.Scripts.DataMapping;

namespace PFE.Core
{
    public class BossInstance
    {
        private readonly BossData data;

        private int damage;
        private int currentHealth;

        public BossInstance(BossData data)
        {
            this.data = data;
        }

        public void Spawn()
        {
            if (data.TryGet(out IBossContainer container))
            {
                container.CanSpawn(data);
            }
        }

        public void TakeDamage()
        {
            if (data.TryGet(out IBossContainer container))
            {
                container.TakeDamage(data, damage,  currentHealth);
            }
        }
    }
}