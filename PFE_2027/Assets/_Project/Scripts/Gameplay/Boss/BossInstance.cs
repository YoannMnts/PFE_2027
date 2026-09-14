using PFE.Core.Scripts;
using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.GameSettings;

namespace PFE.Core
{
    public class BossInstance
    {
        public StageMetric<BossMetric> Metric => data.Metrics;
        
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

        public void AddOrRemoveHealth()
        {
            if (data.TryGet(out IBossContainer container))
            {
                container.TakeDamage(data, damage,  currentHealth);
            }
        }
    }
}