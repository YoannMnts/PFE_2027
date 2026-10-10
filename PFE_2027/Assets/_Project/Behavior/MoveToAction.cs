using PFE.Gameplay.Scripts.NPCs;
using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Move To", 
    story: "[Agent] move to [TargetPosition]", 
    category: "Action", 
    id: "07876e5e97c3ce31f92715ebf4d49cb3")]

// Chases the target until its parent stops it (e.g. the attack branch of a parallel completes):
// it never succeeds on its own.
public partial class MoveToAction : Action
{
    // The destination is only sent again when the target moved further than this (avoids a path request every frame)
    private const float REPATH_DISTANCE = 0.5f;

    [SerializeReference] public BlackboardVariable<RuntimeNpc> agent;
    [SerializeReference] public BlackboardVariable<Vector3> targetPosition;

    // Kept between runs: the node is restarted every tick by the Repeat
    private Vector3 lastDestination;

    protected override Status OnStart()
    {
        if (agent.Value == null)
            return Status.Failure;

        // Also done here: when the parallel ends in the same tick, OnUpdate is never called
        UpdateDestination();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (agent.Value == null)
            return Status.Failure;

        UpdateDestination();
        return Status.Running;
    }

    private void UpdateDestination()
    {
        RuntimeNpc npc = agent.Value;
        NavMeshAgent navMeshAgent = npc.NavMeshAgent;
        Vector3 target = targetPosition.Value;

        bool targetMoved = (target - lastDestination).sqrMagnitude > REPATH_DISTANCE * REPATH_DISTANCE;
        // Path dropped by a knockback (ResetPath), or never set
        bool lostPath = !navMeshAgent.hasPath && !navMeshAgent.pathPending;
        if (!targetMoved && !lostPath)
            return;

        lastDestination = target;
        npc.MoveTo(target);
    }
}
