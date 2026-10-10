using System;
using PFE.Core.Scripts.Pilgrims;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players.Default.Runtime;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : RuntimeNpc<PilgrimInstance, PilgrimData>
    {
        [SerializeField, BoxGroup("References")]
        private CinemachineTargetGroup targetGroup;

        protected override Vector3 TargetPosition => Instance?.TargetDestination ?? transform.position;
        
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
            if (Instance != null)
                Instance.UpdatePosition(transform.position);
        }

    }
}
