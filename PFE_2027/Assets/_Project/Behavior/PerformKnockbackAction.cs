using PFE.Gameplay.Scripts.NPCs;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PerformKnockback", story: "[Agent] takes Knockback", category: "Action", id: "33a2a8a3d88fea10c89873049f0d99de")]
public partial class PerformKnockbackAction : Action
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> Agent;

    protected override Status OnStart()
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

