using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    // Stateless pilgrim behaviour, its state (position...) lives in PilgrimInstance
    public partial class Pilgrim : Npc<PilgrimData, PilgrimInstance>
    {
        protected override PilgrimInstance Create(PilgrimData data, NpcInstanceContext context) => new PilgrimInstance(data, context.splineContainer);

        protected override void Act(PilgrimData data, PilgrimInstance instance)
        {
            
            
            
        }
    }
}
