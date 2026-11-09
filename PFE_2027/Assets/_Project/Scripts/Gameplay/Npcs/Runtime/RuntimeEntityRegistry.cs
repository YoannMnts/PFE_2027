using PFE.Core.Scripts.ECS;
using PFE.Gameplay.Scripts.Entities;

namespace PFE.Gameplay.Scripts.NPCs
{
    public class RuntimeEntityRegistry : SparseSet<IRuntimeEntity>
    {
        public RuntimeEntityRegistry(int capacity) : base(capacity)
        {
        }
    }
}