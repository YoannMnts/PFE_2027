using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct TemplateElementComponent : IElementComponent<TemplateElementData>
    {
        public bool CanTrigger(TemplateElementData data, ComponentContext context)
        {
            return true;
        }

        public void ApplyElement(TemplateElementData data)
        {
            // TODO: logique de l'element
        }
    }
}