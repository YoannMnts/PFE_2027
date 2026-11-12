using PFE.Core.Scripts.ECS;
using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.Components;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Interactions;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyData>, IInteractable
    {
        // Also runs when the NpcGenerator reveals the enemy (SetActive(true)), the instance exists by then
        private void OnEnable() => Follow();
        private void OnDisable() => UnFollow();

        // The first OnEnable runs during the instantiation, before the instance is plugged

        private void Follow()
        {
            //TODO refaire le follow du pilgrim
        }
        
        private void UnFollow()
        {
        }
        
        public void LaunchInteract()
        {
        }
    }
}
