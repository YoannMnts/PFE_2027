using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;

namespace PFE.Gameplay.Scripts.Components
{
    public struct PlayerAllyComponent : IEntityComponent
    {
        public readonly bool isPlayerAlly;
        public PlayerAllyComponent(NpcData data)
        {
            isPlayerAlly = data.IsPlayerAlly;
        }
    }
}