using PFE.Gameplay.Scripts.NPCs;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To", 
    story: "[Agent] move to [TargetPosition]", 
    category: "Action", 
    id: "07876e5e97c3ce31f92715ebf4d49cb3")]

public partial class MoveToAction : Action
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> agent;
    [SerializeReference] public BlackboardVariable<Vector3> targetPosition;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        agent.Value.MoveTo(targetPosition.Value);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

