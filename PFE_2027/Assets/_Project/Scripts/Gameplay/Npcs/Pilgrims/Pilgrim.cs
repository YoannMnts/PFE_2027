using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;
using UnityEngine.Splines;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    // Stateless pilgrim behaviour, its state (position...) lives in PilgrimInstance
    public partial class Pilgrim : Npc<PilgrimData, PilgrimInstance>
    {
        protected override PilgrimInstance Create(PilgrimData data, NpcInstanceContext context) 
            => new PilgrimInstance(data, context.splineContainer, context.crossRoadGameMode.Players);

        protected override void SetupComponent(PilgrimData data, EntityAddress address)
        {
            address.AddComponent<PilgrimTagComponent>();
            address.AddComponent<HealthComponent>();
            address.AddComponent<PlayerAllyComponent>();
        }
    }
}
