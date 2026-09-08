using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.DataMapping.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract class ComponentData : GameDatabaseObject, IData
    {
        [field: SerializeField, Range(0, 50), BoxGroup("Recharge")]
        public int MaxRechargeValue { get; protected set; } = 5;

        [field: SerializeField, Range(0, 50), BoxGroup("Recharge")]
        public int DecrementValue { get; protected set; } = 1;
    }
}