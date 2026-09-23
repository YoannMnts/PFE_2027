using PFE.Core.Scripts.Databases;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Attacks
{
    public abstract class AttackData : GameDatabaseObject, IAttackData
    {
        [field: SerializeField, BoxGroup("Parameters")]
        public int Damage { get; private set; } = 10;
        
        //DamageZone => a la fois la zone de damage durant, et après
            //nbr zone de dégats
            // cooldown
        //Projectile ?? je pense que c'est un script à part qu'on appelle 
            //Cooldown de destruction
            //trajectoire ??
        
        // !
        //accès a la position du player
        
    }
}