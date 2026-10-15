using PFE.Gameplay.Scripts.Interactions;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        private const int MAX_COLLIDERS = 16;
        private const int SPHERE_SIZE = 100;

        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];

        public bool TryGetNearestInteractable<T>(out IInteractable<T> interactable)
            where T : MonoBehaviour
        {
            var count = 
                Physics.OverlapSphereNonAlloc(transform.position, SPHERE_SIZE, overlapResults, hitMask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                var collider = overlapResults[i];
                var target = collider.GetComponentInParent<IInteractable<T>>();
                
                if(target == null)
                    continue;
                
                interactable = target;
                return true;
            }
            
            interactable = null;
            return false;
        }
    }
}