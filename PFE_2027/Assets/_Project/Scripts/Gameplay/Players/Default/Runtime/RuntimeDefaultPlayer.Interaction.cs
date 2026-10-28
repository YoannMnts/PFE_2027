using System;
using System.Collections.Generic;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.Interactions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

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

        private struct NearestInteractable : IComparable<NearestInteractable>
        { 
            public IInteractable Target { get; private set; }
            public int Distance { get; private set; }

            public NearestInteractable(IInteractable target, int distance)
            {
                Target = target;
                Distance = distance;
            }

            public int CompareTo(NearestInteractable other)
            {
                return Distance.CompareTo(other.Distance);
            }
        }
        
        
        public bool TryGetNearestInteractable(out IInteractable interactable)
        {
            using (ListPool<NearestInteractable>.Get(out List<NearestInteractable> interactables))
            {
                Vector3 playerPosition = mesh.position;
                
                //------travail et calcul avec objet runtime-------
                int count = 
                    Physics.OverlapSphereNonAlloc(mesh.position, SPHERE_SIZE,
                        overlapResults, interactMask, 
                        QueryTriggerInteraction.Collide);
                
                //------calcul avec i-------
                for (var i = 0; i < count; i++)
                {
                    var collider = overlapResults[i];
                    var currentTarget = collider.GetComponentInParent<IInteractable>();
                    
                    //-----------DOUANE------------
                    if (currentTarget == null)
                        continue;
                    
                    Vector3 closestPoint = collider.ClosestPointOnBounds(playerPosition);
                    
                    float distance = playerPosition.sqrMagnitude - closestPoint.sqrMagnitude;
                    var currentDistance = Mathf.RoundToInt(distance);

                    interactables.Add(new NearestInteractable(currentTarget, currentDistance));
                }

                if (interactables.Count <= 0)
                {
                    interactable = null;
                    return false;
                }
                
                //le rangement SORT est décider par le Compare
                interactables.Sort();
                interactable = interactables[0].Target;
                return true;
            }
        }
        
        public void Interact()
        {
            if (!TryGetNearestInteractable(out currentInteractable))
            {
                return;
            }
            currentInteractable.LaunchInteract();
        }
    }
}
