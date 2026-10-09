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
    
    private AttackRunner runner;

    protected override Status OnStart()
    {
        RuntimeNpc npc = agent.Value;
        if (npc == null || npc.AttackRunner == null || attack.Value == null)
            return Status.Failure;

        runner = npc.AttackRunner;

        runner.Begin(attack.Value);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return runner.IsRunning ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
        if (runner != null && runner.IsRunning)
            runner.End();

        runner = null;
    }
}

