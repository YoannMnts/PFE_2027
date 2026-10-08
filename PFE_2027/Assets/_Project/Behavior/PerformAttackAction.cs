using PFE.Core.Scripts.Enemy.Attacks;
using System;
using PFE.Gameplay.Scripts.NPCs;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PerformAttack", story: "[Agent] performs [attack]", category: "Action", id: "6e49305e06b28095e7731698dd4f2e08")]
public partial class PerformAttackAction : Unity.Behavior.Action
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> Agent;
    [SerializeReference] public BlackboardVariable<AttackData> Attack;

    protected override Node.Status OnStart()
    {
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

