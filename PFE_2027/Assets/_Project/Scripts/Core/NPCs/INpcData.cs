using PFE.Core.Scripts.DataMapping.Interfaces;
using UnityEngine;

namespace PFE.Core.Scripts.NPCs
{
    // Racine du domaine INpc dans le mapper : toute data de NPC (ennemi, pèlerin...) passe par ici
    public interface INpcData : IData
    {
        public string Name { get; }
        public float MaxHealth { get; }
        public Transform Prefab { get; }
    }
}
