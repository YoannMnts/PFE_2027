using PFE.Core.Scripts.AIPattern;
using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Core
{
    [GenerateContainer]
    public interface IBoss<in TData> : IBehaviour<TData> where TData : IBossData
    {
        [AddToContainer]
        //set the spawning => maybe into scene 
        bool CanSpawn(TData data);
        
        [AddToContainer]
        //set the attack 
        void Attack(TData data);
        
        [AddToContainer]
        //set taken damage
        void TakeDamage(TData data, int damage, int  currentHealth);
        
        [AddToContainer]
        //set death
        void Dying(TData data);
    }
    
}