using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct SwordBasicAttackComponent : IBasicAttackComponent<SwordBasicAttackData>
    {
        public bool CanTrigger(SwordBasicAttackData data, ComponentContext context)
        {
            return true;
        }

        public void ExecuteBasicAttack(SwordBasicAttackData data)
        {
            // TODO
        }
    }
}
