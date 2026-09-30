using System;
using PFE.Gameplay.Scripts.NPCs;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : RuntimeNpc<PilgrimInstance>
    {
        private void FixedUpdate()
        {
            if (instance != null)
                instance.UpdatePosition(transform.position);
        }

        
        
        
        private void LateUpdate()
        {
            MoveTo(instance.TargetDestination);
        }
    }
}
