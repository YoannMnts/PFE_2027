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
    [SerializeReference] public BlackboardVariable<RuntimeNpc> agent;
    [SerializeReference] public BlackboardVariable<AttackData> attack;

    protected override Status OnStart()
    {
        Debug.Log("Start Attack");
        
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        Debug.Log("Update Attack");

        return Status.Waiting;
    }

    protected override void OnEnd()
    {
        Debug.Log("End Attack");
    }
}

