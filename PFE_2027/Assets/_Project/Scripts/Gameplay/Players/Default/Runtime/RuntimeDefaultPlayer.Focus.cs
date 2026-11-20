using System;
using System.Collections.Generic;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        private enum FocusState
        {
            Lock,
            Unlock,
            TempUnlock
        }

        // One enemy found in the focus radius, with its selection cost (lower is better)
        private readonly struct FocusTarget : IComparable<FocusTarget>
        {
            public readonly RuntimeEnemy enemy;
            public readonly float cost;

            public FocusTarget(RuntimeEnemy enemy, float cost)
            {
                this.enemy = enemy;
                this.cost = cost;
            }

            // Ascending order: after Sort(), index 0 is the best target
            public int CompareTo(FocusTarget other) => cost.CompareTo(other.cost);
        }
        
        private const int MAX_RESULTS = 16;
        private const float MIN_SQR_DISTANCE = 0.0001f;
        private readonly Collider[] focusResults = new Collider[MAX_RESULTS];
        private readonly List<FocusTarget> focusTargets = new();
        
        private RuntimeEnemy currentFocusTarget;
        private FocusState focusState = FocusState.Unlock;
        private readonly ChannelKey focusChannel = ChannelKey.GetUniqueChannelKey();

        public void Focus()
        {
            if (focusState == FocusState.Lock)
            {
                Unfocus();
                return;
            }

            if (!TryFindFocusTarget(mesh, Player.Data.FocusRadius, hitMask, out var target))
                return;

            currentFocusTarget = target;
            focusState = FocusState.Lock;
            targetFacingDirection.AddPriority(focusChannel, PriorityTags.Highest, GetFocusDirection());
        }

        private void Unfocus()
        {
            focusState = FocusState.Unlock;
            targetFacingDirection.RemovePriority(focusChannel);
            currentFocusTarget = null;
        }

        public void SwitchFocus(Vector2 direction)
        {
            if(focusState != FocusState.Lock)
                return;
            
            if (!TryFindFocusTarget(mesh.position, direction, Player.Data.FocusRadius, hitMask, out var target))
                return;

            currentFocusTarget = target;
            targetFacingDirection.AddPriority(focusChannel, PriorityTags.Highest, GetFocusDirection());
        }

        private void UpdateFocus()
        {
            if (focusState != FocusState.Lock)
                return;

            if (currentFocusTarget == null)
            {
                Unfocus();
                return;
            }

            targetFacingDirection.Write(focusChannel, GetFocusDirection());
        }

        private Vector3 GetFocusDirection()
        {
            Vector3 direction = currentFocusTarget.transform.position - mesh.position;
            direction.y = 0f;
            return direction;
        }

        private bool TryFindFocusTarget(Transform owner, float radius, LayerMask mask, out RuntimeEnemy target) 
            => TryFindFocusTarget(owner.position, owner.forward, radius, mask, out target);

        private bool TryFindFocusTarget(Vector3 origin, Vector3 forward, float radius, LayerMask mask, out RuntimeEnemy target)
        {
            target = null;

            forward.y = 0f;
            if (forward.sqrMagnitude < MIN_SQR_DISTANCE)
                return false;

            CollectFocusTargets(origin, forward, radius, mask);
            if (focusTargets.Count == 0)
                return false;

            focusTargets.Sort();
            target = focusTargets[0].enemy;
            return true;
        }

        // Fills focusTargets with every enemy in the radius and its cost. Not sorted
        private void CollectFocusTargets(Vector3 origin, Vector3 forward, float radius, LayerMask mask)
        {
            focusTargets.Clear();
            var data = Player.Data;

            int count = Physics.OverlapSphereNonAlloc(origin, radius, focusResults, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                RuntimeEnemy enemy = focusResults[i].GetComponentInParent<RuntimeEnemy>();
                if (enemy == null || ContainsFocusTarget(enemy))
                    continue;

                Vector3 toEnemy = enemy.transform.position - origin;
                toEnemy.y = 0f;
                if (toEnemy.sqrMagnitude < MIN_SQR_DISTANCE)
                    continue;

                // Both parts in [0, 1]: 0 = straight ahead / right next to the player
                float angle01 = Vector3.Angle(forward, toEnemy) / 180f;
                float distance01 = Mathf.Clamp01(toEnemy.magnitude / radius);

                float cost = angle01 * data.FocusAngleWeight + distance01 * data.FocusDistanceWeight;
                focusTargets.Add(new FocusTarget(enemy, cost));
            }
        }

        // An enemy with several colliders is returned once per collider by the overlap
        private bool ContainsFocusTarget(RuntimeEnemy enemy)
        {
            for (int i = 0; i < focusTargets.Count; i++)
            {
                if (focusTargets[i].enemy == enemy)
                    return true;
            }

            return false;
        }
    }
}