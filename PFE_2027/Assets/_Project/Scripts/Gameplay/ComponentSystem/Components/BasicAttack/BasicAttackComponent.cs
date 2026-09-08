using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct BasicAttackComponent : IComponent<BasicAttackComponentData>, IBasicAttackComponent<BasicAttackComponentData>
    {
        public bool Trigger(BasicAttackComponentData data, ComponentContext context)
        {
            ExecuteBasicAttack(data);
            return true;
        }

        public void ExecuteBasicAttack(BasicAttackComponentData data)
        {
            // TODO: logique de l'attaque de base
        }
    }
}