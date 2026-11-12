using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    // Stateless pilgrim behaviour, its state (position...) lives in PilgrimInstance
    public partial class Pilgrim : Npc<PilgrimData>
    {
        public override void SetupEntity(PilgrimData data, EntityAddress address)
        {
            base.SetupEntity(data, address);
            
            address.AddComponent<PilgrimTagComponent>(new(data));
            address.AddComponent<HealthComponent>(new(data.MaxHealth));
            address.AddComponent<PlayerAllyComponent>(new());
            address.AddComponent(new AIMovementComponent());
        }

        public override Vector3 RedirectTargetPosition(PilgrimData data, EntityAddress address, Vector3 goal)
        {
            return goal;
        }
    }
}
