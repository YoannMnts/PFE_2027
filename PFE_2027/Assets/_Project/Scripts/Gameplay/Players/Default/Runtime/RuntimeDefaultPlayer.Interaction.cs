using System;
using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
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
        
        //ce commentaire si t'as pas la vision c'est ok, mais on pourrait passer cette variable
        //en Priority<IInteractable> est ajouter chaque interactable avec leur distance comme priorité
        //N.B : si tu fais ça on a plus besoin de (out IInteractable) dans le TryGetNearest
        private IInteractable currentInteractable;
        
        private ChannelKey keyState;
        
        //a decaller dans le runtimePilgrim je pense, comme ça le joueur
        //peut toujours interagir mais juste le pilgrim ne reagit que a la CampfirePhase
        public Condition isInteractionPossible;
        
        //ces deux conditions peuvent etre supprimer aussi,
        //on peut passer par les phases pour savoir où on en est
        public Condition isInProtectPhase;
        public Condition isInCampfirePhase;

        //a decaller dans le Awake, la regle c'est "si c'est interne alors awake, si c'est externe alors start"
        //comme ça lorsqu'une autre classe doit acceder aux conditions on est assuré qu'elles sont créées
        private void Start()
        {
            isInteractionPossible = new Condition(false);
            isInProtectPhase = new Condition(false);
            isInCampfirePhase = new Condition(false);
        }
        
        //la méthode s’appelle Nearest,
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
                
                /*
                 * la formule pour avoir la distance entre deux points A et B c'est
                 * Distance = B - A
                 * avec A ton point de depart (le joueur)
                 * et B l'arrivée (l'interactible)
                 */
                
                interactable = target;
                isInteractionPossible.Write(keyState, true);
                Debug.Log($"{interactable}");
                return true;
            }
            
            interactable = null;
            isInteractionPossible.Write(keyState, false);
            return false;
        }
        
        public void Interact()
        {
            if (TryGetNearestInteractable(out currentInteractable))
            {
                //on arrive ici uniquement si le TryGetNearestInteractable a renvoyé true,
                //donc logiquement les conditions renverront jamais false
                if (!isInteractionPossible)
                    return;

                if (!isInProtectPhase || !isInCampfirePhase)
                    return;
                
                currentInteractable.IsInteractable(true);
                Debug.Log($"{currentInteractable}");
            }
        }
    }
}
