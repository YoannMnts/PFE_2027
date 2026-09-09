using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct TemplatePassiveComponent : IPassiveComponent<TemplatePassiveData>
    {
        public bool CanTrigger(TemplatePassiveData data, ComponentContext context)
        {
            return true;
        }

        public void ApplyPassive(TemplatePassiveData data)
        {
            // TODO: logique du passif
        }
    }
}