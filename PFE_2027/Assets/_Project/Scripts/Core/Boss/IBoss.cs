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
        void Spawn(TData data);
        
        [AddToContainer]
        //set the attack 
        void Attack(TData data);
    }
}