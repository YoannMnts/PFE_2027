using System;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players.Default.Runtime;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : RuntimeNpc<PilgrimInstance>
    {
        [SerializeField]
        private CinemachineTargetGroup targetGroup;
        
        public void AddMemberToCmGroup(Transform member, float weight = 0, float radius = 0)
        {
            targetGroup.AddMember(member, weight, radius);
        }

        public void RemoveMemberFromCmGroup(Transform member)
        {
            targetGroup.RemoveMember(member);
        }
        
        private void FixedUpdate()
        {
            if (instance != null)
                instance.UpdatePosition(transform.position);
        }

        
        private void LateUpdate()
        {
            MoveTo(instance.TargetDestination);
        }
    }
}
