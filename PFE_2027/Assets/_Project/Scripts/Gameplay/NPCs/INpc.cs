using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Domaine unique du mapper pour tous les NPC -> génère INpcContainer
    [GenerateContainer]
    public interface INpc<in TData> : IBehaviour<TData> where TData : INpcData
    {
        [AddToContainer]
        //crée l'instance (état runtime) propre à ce type de NPC et la branche sur son runtime spawné
        //renvoie null si le runtime ne correspond pas au type d'instance
        INpcInstance CreateInstance(TData data, Transform runtime);

        [AddToContainer]
        //autorise ou non le spawn
        bool CanSpawn(TData data);

        [AddToContainer]
        //comportement actif du NPC (ennemi => attaque, pèlerin => avance...)
        void Act(TData data, INpcInstance instance);

        [AddToContainer]
        //calcule la nouvelle vie (amount négatif = dégâts)
        float ModifyHealth(TData data, int amount, float currentHealth);

        [AddToContainer]
        //appelé quand la vie tombe à 0
        void Dying(TData data, INpcInstance instance);
    }
}
