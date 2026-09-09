using PFE.Core.Scripts.ComponentSystem;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public partial struct SubAttackComponent : ISubAttackComponent<SubAttackComponentData>
    {
        public bool Trigger(SubAttackComponentData data, ComponentContext context)
        {
            ExecuteSubAttack(data);
            return true;
        }

        public void ExecuteSubAttack(SubAttackComponentData data)
        {
            // TODO: logique de la sub attack
        }
    }
}