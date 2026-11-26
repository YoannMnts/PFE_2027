using PFE.Core.Scripts.Enemy;
using PFE.Gameplay.Scripts.CrossRoadGameModes;
using PFE.Gameplay.Scripts.Interactions;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyInstance, EnemyData>, IInteractable
    {
        [field : SerializeField] public Transform focusModeUI { get; private set; }
        public RuntimeEnemy Interactable => this;

        // Pilgrim followed while this enemy is active (given by its instance)
        private PilgrimInstance followedPilgrim;

        // Also runs when the NpcGenerator reveals the enemy (SetActive(true)), the instance exists by then
        protected override void OnEnable()
        {
            base.OnEnable();
            Follow(Instance?.Pilgrim);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Follow(null);
        }

        // The first OnEnable runs during the instantiation, before the instance is plugged
        protected override void OnSetup(CrossRoadGameModeContext context)
        {
            base.OnSetup(context);

            focusModeUI.gameObject.SetActive(false);
            
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


        protected override void OnDestroy()
        {
        }

        public void LaunchInteract()
        {
            //comportement du focus
        }
    }
}
