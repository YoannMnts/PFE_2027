using PFE.Gameplay.Scripts.NPCs;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsKnockedBack", story: "[Agent] is KnockedBack", category: "Conditions", id: "bf6bc34bce126f318006919b1f1b3dd1")]
public partial class IsKnockedBackCondition : Condition
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> agent;

    public override bool IsTrue()
    {
        return agent.Value.IsKnockedBack;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
