using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct BasicAttackComponent : IBasicAttackComponent<TemplateBasicAttackData>
    {
        public bool Trigger(TemplateBasicAttackData data, ComponentContext context)
        {
            ExecuteBasicAttack(data);
            return true;
        }

        public void ExecuteBasicAttack(TemplateBasicAttackData data)
        {
            // TODO: logique de l'attaque de base
        }
    }
}