using System;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Gameplay.Scripts.NPCs;
using Unity.Behavior;
using Unity.VisualScripting;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition
    (name: "CanAttack", 
        story: "distance between [agent] and [targetPosition] <= [attackData]", 
        category: "Conditions", 
        id: "3ab75e3132b4712113ae6454343dc370")]
public partial class CanAttackCondition : Condition
{
    [SerializeReference] public BlackboardVariable<RuntimeNpc> agent;
    [SerializeReference] public BlackboardVariable<AttackData> attackData;
    [SerializeReference] public BlackboardVariable<Vector3> targetPosition;
    
    private float sqrDistance;

    public override bool IsTrue()
    {
        if(attackData.Value == null)
            return false;
        
        bool isInRange = sqrDistance <= attackData.Value.AttackRange * attackData.Value.AttackRange;
        return isInRange;
    }
    

    public override void OnStart()
    {
        var agentPosition = agent.Value.transform.position;
        var offsetDistance = targetPosition.Value - agentPosition;

        sqrDistance = offsetDistance.sqrMagnitude;
    }

    public override void OnEnd()
    {
    }
}
