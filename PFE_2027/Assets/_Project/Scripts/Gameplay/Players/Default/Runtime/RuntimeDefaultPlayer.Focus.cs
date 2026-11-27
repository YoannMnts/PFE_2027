using System;
using System.Collections.Generic;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Priorities;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using UnityEngine;
using UnityEngine.Pool;

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
        // Stick magnitude needed to switch target, and the one under which the stick counts as released
        private const float SWITCH_FOCUS_THRESHOLD = 0.7f;
        private const float SWITCH_FOCUS_RESET = 0.3f;
        private readonly Collider[] focusResults = new Collider[MAX_RESULTS];
        
        private RuntimeEnemy currentFocusTarget;
        private RuntimeEnemy lastFocusTarget;

        private FocusState focusState = FocusState.Unlock;
        private readonly ChannelKey focusChannel = ChannelKey.GetUniqueChannelKey();
        // One switch per flick: re-armed when the stick goes back to the center
        private bool canSwitchFocus = true;

        public void Focus()
        {
            if (focusState == FocusState.Lock)
            {
                lastFocusTarget = currentFocusTarget;
                currentFocusTarget.debugFocusModeUI.gameObject.SetActive(false);
                
                Unfocus();
                return;
            }

            if (!TryFindFocusTarget(mesh, Player.Data.FocusRadius, hitMask, out var target))
            {
                currentFocusTarget = null;
                return;
            }
            
            currentFocusTarget = target;
            focusState = FocusState.Lock;
            targetFacingDirection.AddPriority(focusChannel, PriorityTags.Highest, GetFocusDirection());
            currentFocusTarget.debugFocusModeUI.gameObject.SetActive(true);
        }

        private void Unfocus()
        {
            focusState = FocusState.Unlock;
            targetFacingDirection.RemovePriority(focusChannel);
            currentFocusTarget = null;
        }

        // Called with the right stick value, including (0, 0) when it is released
        public void SwitchFocus(Vector2 input)
        {
            float magnitude = input.magnitude;
            
            if (magnitude < SWITCH_FOCUS_RESET)
            {
                canSwitchFocus = true;
                return;
            }

            if (!canSwitchFocus || magnitude < SWITCH_FOCUS_THRESHOLD || focusState != FocusState.Lock)
            {
                return;
            }

            canSwitchFocus = false;

            // Stick space (x, y) to world space (x, 0, z), turned like the view, as CharacterRun does for the movement
            Vector3 direction = Quaternion.Euler(0f, view.eulerAngles.y, 0f) * new Vector3(input.x, 0f, input.y);
            
            if (!TryFindFocusTarget(mesh.position, direction, Player.Data.FocusRadius, hitMask, out var target))
            {
                lastFocusTarget = currentFocusTarget;
                return;
            }
            
            lastFocusTarget = currentFocusTarget;
            currentFocusTarget = target;
            
            lastFocusTarget.debugFocusModeUI.gameObject.SetActive(false);
            currentFocusTarget.debugFocusModeUI.gameObject.SetActive(true);
            
            targetFacingDirection.Write(focusChannel, GetFocusDirection());
        }

        private void UpdateFocus()
        {
            if (focusState != FocusState.Lock)
            {
                return;
            }

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
            using (ListPool<FocusTarget>.Get(out List<FocusTarget> focusTargets))
            {
                target = null;

                forward.y = 0f;
                if (forward.sqrMagnitude < MIN_SQR_DISTANCE)
                    return false;

                CollectFocusTargets(origin, forward, radius, mask, focusTargets);
                if (focusTargets.Count == 0)
                    return false;

                focusTargets.Sort();
                target = focusTargets[0].enemy;
                return true;
            }
        }

        // Fills focusTargets with every enemy in the radius and its cost. Not sorted
        private void CollectFocusTargets(Vector3 origin, Vector3 forward, float radius, LayerMask mask, List<FocusTarget> focusTargets)
        {
            var data = Player.Data;

            int count = Physics.OverlapSphereNonAlloc(origin, radius, focusResults, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                RuntimeEnemy enemy = focusResults[i].GetComponentInParent<RuntimeEnemy>();
                if (enemy == null || ContainsFocusTarget(enemy, focusTargets) || enemy == currentFocusTarget)
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
        private bool ContainsFocusTarget(RuntimeEnemy enemy, List<FocusTarget> focusTargets)
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