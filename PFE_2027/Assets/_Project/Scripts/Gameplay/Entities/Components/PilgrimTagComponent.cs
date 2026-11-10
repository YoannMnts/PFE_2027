using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Core.Scripts.Pilgrims;

namespace PFE.Gameplay.Scripts.Components
{
    public struct PilgrimTagComponent : IEntityComponent
    {
        public readonly PilgrimData data;

        public PilgrimTagComponent(NpcData data)
        {
            if (data is PilgrimData pilgrimData)
            {
                this.data = pilgrimData;
                return;
            }
            
            this.data = null;
        }
    }
}