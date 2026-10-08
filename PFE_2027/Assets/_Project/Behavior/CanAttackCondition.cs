using System;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Gameplay.Scripts.NPCs;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition
    (name: "CanAttack", 
        story: "[agent] can attack with [attackData] ", 
        category: "Conditions", 
        id: "3ab75e3132b4712113ae6454343dc370")]
public partial class CanAttackCondition : Condition
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> Agent;
    [SerializeReference] public BlackboardVariable<AttackData> AttackData;
    
    
    
    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
