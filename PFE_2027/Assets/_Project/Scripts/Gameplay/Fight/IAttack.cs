using PFE.Core.Scripts.DataMapping.Attributes;
using PFE.Core.Scripts.DataMapping.Interfaces;
using UnityEngine;

namespace PFE.Core.Scripts.Enemy.Attacks
{
    [GenerateContainer]
    public interface IAttack<in TData> : IBehaviour<TData> where TData : AttackData
    {
        
    }
}