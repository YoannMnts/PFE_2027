using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct PassiveComponent : IPassiveComponent<PassiveComponentData>
    {
        public bool Trigger(PassiveComponentData data, ComponentContext context)
        {
            ApplyPassive(data);
            return true;
        }

        public void ApplyPassive(PassiveComponentData data)
        {
            // TODO: logique du passif
        }
    }
}