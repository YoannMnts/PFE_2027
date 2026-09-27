using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    // Behaviour stateless du pèlerin, son état (position...) est dans PilgrimInstance
    public partial class Pilgrim : Npc<PilgrimData, PilgrimInstance>
    {
        protected override PilgrimInstance Create(PilgrimData data) => new PilgrimInstance(data);

        public override void Act(PilgrimData data, INpcInstance instance)
        {
            //TODO avancer le long du chemin
        }
    }
}
