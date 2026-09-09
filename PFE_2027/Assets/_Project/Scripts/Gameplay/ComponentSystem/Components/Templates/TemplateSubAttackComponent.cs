using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct TemplateSubAttackComponent : ISubAttackComponent<TemplateSubAttackData>
    {
        public bool CanTrigger(TemplateSubAttackData data, ComponentContext context)
        {
            return true;
        }

        public void ExecuteSubAttack(TemplateSubAttackData data)
        {
            // TODO: logique de la sub attack
        }
    }
}