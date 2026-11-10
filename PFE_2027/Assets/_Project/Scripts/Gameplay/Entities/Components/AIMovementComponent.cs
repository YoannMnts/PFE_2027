using PFE.Core.Scripts.DataMapping;
using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.NPCs;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Components
{
    public struct AIMovementComponent : IEntityComponent
    {
        public Vector3 TargetPosition { get; private set; }
        public Vector3 Goal { get; private set; }

        public void SetGoal(NpcData data, Vector3 goal)
        {
            Goal = goal;
            
            if(!data.TryGet<INpcContainer>(out var container))
            {
                TargetPosition = goal;
                return;
            }
            
            //TargetPosition = container.RedirectTargetPosition(data, this, goal);
        }
    }
}