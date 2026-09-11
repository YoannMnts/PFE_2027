using UnityEngine;
using Sirenix.OdinInspector;

namespace PFE.Core.Scripts.ComponentSystem
{
    public abstract class RechargeableComponentData : ComponentData
    {
        [field: SerializeField, Range(0, 50), BoxGroup("Recharge")]
        public int MaxRechargeValue { get; protected set; } = 5;

        [field: SerializeField, Range(-50, 0), BoxGroup("Recharge")]
        public int DecrementValue { get; protected set; } = -1;
    }
}
