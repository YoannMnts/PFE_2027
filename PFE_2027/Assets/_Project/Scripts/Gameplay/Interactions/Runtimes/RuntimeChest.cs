using Helteix.ChanneledProperties;
using Helteix.ChanneledProperties.Conditions;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Interactions.Chests
{
    public class RuntimeChest : MonoBehaviour, IInteractable
    {
        //dans les classes interactible t'as pas besoin de check si tu peux interagir
        //pars du principe que "si t'es arrivé la c'est que c'est possible"
        //N.B. :
        //je pense renomme la methode en Interact ou LaunchInteraction
        //et ça arrive souvent qu'on passe le gars qui a lancer l'interaction dans les paramètres de la methodes
        public void LaunchInteract()
        {
            //pour les interactions c'est dur de vraiment separer logique et runtime,
            //donc c'est ok si c'est le runtime qui appel des methodes de la logique
            //tant qu'il la drive pas completement bien sur
            
            //TODO ici trouver un moyen de bien séparer le runtime de la logic
            /*
            if (interactionPossibility)
            {
                instance.TryLaunchInteractable(interactionPossibility);
                return true;
            }
            */
        }
    }
}