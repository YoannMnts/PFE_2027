using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct TemplateBasicAttackComponent : IBasicAttackComponent<TemplateBasicAttackData>
    {
        public bool CanTrigger(TemplateBasicAttackData data, ComponentContext context)
        {
            return true;
        }

        public void ExecuteBasicAttack(TemplateBasicAttackData data)
        {
            // TODO: logique de l'attaque de base
        }
    }
}