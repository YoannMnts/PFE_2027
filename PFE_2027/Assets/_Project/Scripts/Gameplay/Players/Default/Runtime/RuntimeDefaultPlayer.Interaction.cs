using PFE.Gameplay.Scripts.Interactions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        private const int MAX_COLLIDERS = 16;
        private const int SPHERE_SIZE = 2;

        [SerializeField, BoxGroup("Interaction")]
        private LayerMask interactMask;
        
        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];
        private IInteractable currentInteractable;
        
        
        //TODO Une remarque au passage : la méthode s’appelle Nearest,
        //mais elle prend le premier collider trouvé, pas le plus proche. Avec un
        //pèlerin et un ennemi à portée, la cible dépendra de l’ordre renvoyé par la physique.
        public bool TryGetNearestInteractable(out IInteractable interactable)
        {
            int count = 
                Physics.OverlapSphereNonAlloc(mesh.transform.position, SPHERE_SIZE, overlapResults, interactMask, QueryTriggerInteraction.Collide);
            
            Debug.Log($"{count}");
            
            for (int i = 0; i < count; i++)
            {
                var collider = overlapResults[i];
                var target = collider.GetComponentInParent<IInteractable>();
                
                if(target == null)
                    continue;
                
                interactable = target;
                Debug.Log($"{interactable}");
                return true;
            }
            
            interactable = null;
            return false;
        }
        
        public void Interact()
        {
            if (TryGetNearestInteractable(out currentInteractable))
            {
                //TODO ici le true doit etre modifiable en fonction du state
               currentInteractable.IsInteractable(true);
               Debug.Log($"{currentInteractable}");
            }
        }
    }
}
