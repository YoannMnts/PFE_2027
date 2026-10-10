using PFE.Gameplay.Scripts.NPCs;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PerformKnockback", story: "[Agent] takes Knockback", category: "Action", id: "33a2a8a3d88fea10c89873049f0d99de")]
// Decision side of the knockback: the physics run in RuntimeNpc.Update, this node cancels the current
// attack and keeps the branch busy until the NPC stops sliding.
public partial class PerformKnockbackAction : Action
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> Agent;
    
    private RuntimeNpc runtimeNpc;

    protected override Status OnStart()
    {
        runtimeNpc = Agent.Value;
        if (runtimeNpc == null)
            return Status.Failure;

        // A hit cancels the current attack (not every NPC has an AttackRunner)
        runtimeNpc.AttackRunner?.End();
        return runtimeNpc.IsKnockedBack ? Status.Running : Status.Success;
    }

    protected override Status OnUpdate()
    {
        return runtimeNpc.IsKnockedBack ? Status.Running : Status.Success;
    }

    protected override void OnEnd()
    {
        runtimeNpc = null;
    }
}
