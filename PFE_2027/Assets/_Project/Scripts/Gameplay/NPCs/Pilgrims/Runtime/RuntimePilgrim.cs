using System;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Players.Default.Runtime;
using UnityEngine;
using UnityEngine.AI;

namespace PFE.Gameplay.Scripts.Pilgrims
{
    public class RuntimePilgrim : RuntimeNpc<PilgrimInstance>
    {
        private RuntimeDefaultPlayer runtimePlayer;

        protected override void Awake()
        {
            base.Awake();
            runtimePlayer = GetComponentInParent<RuntimeDefaultPlayer>();
        }

        private void OnEnable()
        {
            runtimePlayer?.AddMemberToCmGroup(transform);
        }

        private void OnDisable()
        {
            runtimePlayer?.RemoveMemberFromCmGroup(transform);
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
