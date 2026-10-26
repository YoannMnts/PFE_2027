using Helteix.ChanneledProperties.Priorities;
using PFE.Gameplay.Scripts.Interactions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        private const int MAX_COLLIDERS = 16;
        private const int SPHERE_SIZE = 2;
        private const int CONVERSION_CENTIMETERS = 100;

        [SerializeField, BoxGroup("Interaction")]
        private LayerMask interactMask;
        
        private readonly Collider[] overlapResults = new Collider[MAX_COLLIDERS];
        public Priority<IInteractable> CurrentInteractable { get; private set; }
        
        
        private void Awake()
        {
            CurrentInteractable = new Priority<IInteractable>();
        }
        
        public bool TryGetNearestInteractable()
        {
            CurrentInteractable.Clear();
            
            Vector3 playerPosition = mesh.position;
            int count = 
                Physics.OverlapSphereNonAlloc(mesh.position, SPHERE_SIZE,
                    overlapResults, interactMask, 
                    QueryTriggerInteraction.Collide);
            
            Debug.Log($"{count}");
            
            for (var i = 0; i < count; i++)
            {
                var collider = overlapResults[i];
                var target = collider.GetComponentInParent<IInteractable>();

                
                //-----------DOUANE------------
                if (target == null)
                    continue;
                

                //calculer distance entre joueur et l'interactible
                Vector3 closestPoint = collider.ClosestPointOnBounds(playerPosition);
                float distance = Vector3.Distance(playerPosition, closestPoint);
                
                
                //inverser pour que la plus petite distance soit la plus grande propriété + centimètres
                var preciseDistance = Mathf.Clamp((SPHERE_SIZE - distance) * CONVERSION_CENTIMETERS,
                    0, 
                    SPHERE_SIZE * CONVERSION_CENTIMETERS);
                
                //arrondir
                var finalPriority = Mathf.RoundToInt(preciseDistance);
                
                Debug.Log($"{target.GetType().Name} : {finalPriority}");
                CurrentInteractable.AddPriority(collider, finalPriority, target);                
            }

            if (CurrentInteractable.ChannelCount == 0)
            {
                return false;
            }

            return true;
        }
        
        public void Interact()
        {
            if (!TryGetNearestInteractable())
            {
                return;
            }
            CurrentInteractable.Value.LaunchInteract();
        }
    }
}
