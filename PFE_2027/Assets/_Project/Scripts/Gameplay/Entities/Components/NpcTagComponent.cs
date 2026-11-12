using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Components
{
    public readonly struct NpcTagComponent : IEntityComponent
    {
        public readonly NpcData npcData;

        public NpcTagComponent(NpcData npcData)
        {
            this.npcData = npcData;
        }
    }
}