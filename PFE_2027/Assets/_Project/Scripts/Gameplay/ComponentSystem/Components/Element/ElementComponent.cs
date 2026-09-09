using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct ElementComponent : IElementComponent<ElementComponentData>
    {
        public bool Trigger(ElementComponentData data, ComponentContext context)
        {
            ApplyElement(data);
            return true;
        }

        public void ApplyElement(ElementComponentData data)
        {
            // TODO: logique de l'element
        }
    }
}