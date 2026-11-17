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
        
        private const int MAX_RESULTS = 16;
        private readonly Collider[] focusResults = new Collider[MAX_RESULTS];
        
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
            => TryFindFocusTarget(owner.position, owner.transform.forward, radius, mask, out target);

        private bool TryFindFocusTarget(Vector3 origin, Vector3 forward, float radius, LayerMask mask, out RuntimeEnemy target)
        {
            target = null;

            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return false;
            forward.Normalize();

            float bestDot = int.MinValue;

            int count = Physics.OverlapSphereNonAlloc(origin, radius, focusResults, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                RuntimeEnemy enemy = focusResults[i].GetComponentInParent<RuntimeEnemy>();
                if (enemy == null)
                    continue;

                Vector3 toEnemy = enemy.transform.position - origin;
                toEnemy.y = 0f;

                float sqrDistance = toEnemy.sqrMagnitude;
                if (sqrDistance < 0.0001f)
                    continue;

                float dot = Vector3.Dot(forward, toEnemy / Mathf.Sqrt(sqrDistance));
                if (dot <= bestDot)
                    continue;

                bestDot = dot;
                target = enemy;
            }

            return target != null;
        }
    }
}