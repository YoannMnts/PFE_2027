using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;
using PFE.Core.Scripts.Enemy;

namespace PFE.Gameplay.Scripts.Enemy
{
    [GenerateContainer]
    public interface IEnemy<in TData> : IBehaviour<TData> where TData : IEnemyData
    {
        [AddToContainer]
        //set the spawning => maybe into scene 
        bool CanSpawn(TData data);
        
        [AddToContainer]
        //set the attack 
        void Attack(TData data);
        
        [AddToContainer]
        //set taken damage
        float ModifyHealth(TData data, int damage, float  currentHealth);
        
        [AddToContainer]
        //set death
        void Dying(TData data);
    }
    
}