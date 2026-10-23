using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.Interactions;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyInstance, EnemyData>, IInteractable
    {
        public RuntimeEnemy Interactable => this;

        // Pilgrim followed while this enemy is active (given by its instance)
        private PilgrimInstance followedPilgrim;

        // Also runs when the NpcGenerator reveals the enemy (SetActive(true)), the instance exists by then
        private void OnEnable() => Follow(Instance?.Pilgrim);
        private void OnDisable() => Follow(null);

        // The first OnEnable runs during the instantiation, before the instance is plugged
        protected override void OnSetup()
        {
            base.OnSetup();

            if (isActiveAndEnabled)
                Follow(Instance.Pilgrim);
        }

        private void Follow(PilgrimInstance pilgrim)
        {
            if (followedPilgrim != null)
                followedPilgrim.OnPositionChanged -= OnPilgrimMoved;

            followedPilgrim = pilgrim;
            if (followedPilgrim == null)
                return;

            followedPilgrim.OnPositionChanged += OnPilgrimMoved;

            // Goal given right away instead of waiting for the next pilgrim move
            OnPilgrimMoved(followedPilgrim.CurrentPosition);
        }

        // The runtime only forwards what happens in the world; the instance and its behaviour pick the target
        protected virtual void OnPilgrimMoved(Vector3 pilgrimPosition) => Instance?.SetGoal(pilgrimPosition);
        
        public bool TryLaunchInteractable()
        {
            //TODO ici le focus
            return false;
        }
    }
}
