using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    // Rules of the CampfirePilgrim pilgrim (stateless, its state lives in PilgrimInstance).
    // Override the Npc defaults here: RedirectTargetPosition, ModifyHealth, Dying...
    public partial class CampfirePilgrim : Npc<CampfirePilgrimData, PilgrimInstance>
    {
        protected override PilgrimInstance Create(CampfirePilgrimData data, NpcInstanceContext context)
            => new PilgrimInstance(data, context.splineContainer);
    }
}
