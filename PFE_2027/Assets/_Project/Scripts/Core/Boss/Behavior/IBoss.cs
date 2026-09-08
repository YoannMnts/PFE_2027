using PFE.Core.DataMapping;
using PFE.Core.Scripts.AIPattern;

namespace PFE.Core
{
    [GenerateContainer]
    public interface IBoss<TData> : IBehaviour<TData> where TData : IBossData
    {
        [AddToContainer]
        //set the spawning => maybe into scene 
        void Spawn(TData data);
        
        [AddToContainer]
        //set the movement inherited by the pattern
        void Move(TData data, MovePattern movePattern);
        
        [AddToContainer]
        //set the attack inherited by the pattern 
        void Attack(TData data, AttackPattern attackPattern);
    }
}