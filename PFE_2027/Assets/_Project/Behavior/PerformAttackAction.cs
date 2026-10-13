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
    
    private HitboxRunner runner;
    private AttackData started;

    protected override Status OnStart()
    {
        RuntimeNpc npc = agent.Value;
        if (npc == null || npc.HitboxRunner == null || attack.Value == null)
            return Status.Failure;

        runner = npc.HitboxRunner;
        started = attack.Value;

        runner.Begin(started);
        return Status.Running;
    }

    // Success as soon as the runner no longer plays this attack (finished, or replaced by a pushback...)
    protected override Status OnUpdate()
    {
        return IsPlayingStartedAttack() ? Status.Running : Status.Success;
    }

    // Only cancels its own attack: the runner may already be playing something else (pushback)
    protected override void OnEnd()
    {
        if (IsPlayingStartedAttack())
            runner.End();

        runner = null;
        started = null;
    }

    private bool IsPlayingStartedAttack() =>
        runner != null && runner.IsRunning && runner.Current == started;
}

