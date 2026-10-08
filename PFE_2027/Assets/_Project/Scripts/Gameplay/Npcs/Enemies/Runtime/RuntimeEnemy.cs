using System;
using Helteix.Tools.Phases;
using PFE.Gameplay.Scripts.CrossRoadGameModes.Phases;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Gameplay.Scripts.Pilgrims;
using Sirenix.OdinInspector;
using Unity.Behavior;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy.Runtime
{
    [RequireComponent(typeof(BehaviorGraphAgent))]
    public abstract class RuntimeEnemy : RuntimeNpc<EnemyInstance>, IPhaseListener<ProtectPilgrimPhase>
    {
        private const string BEHAVIOR_SELF_NPC = "SelfNpc";
        private const string BEHAVIOR_PILGRIM_POSITION = "PilgrimPosition";
        private const string BEHAVIOR_ATTACK_DATA = "AttackData";
            
        private BehaviorGraphAgent behaviorGraphAgent;
        private Vector3 pilgrimPosition;

        protected override void Awake()
        {
            base.Awake();
            behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        }

        private void OnEnable()
        {
            this.Register();
        }

        private void OnDisable()
        {
            this.Unregister();
        }

        protected override void OnSetup()
        {
            base.OnSetup();
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_SELF_NPC, this);
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_ATTACK_DATA, Instance.data.AttackData);
        }

        private void LateUpdate()
        {
            behaviorGraphAgent.SetVariableValue(BEHAVIOR_PILGRIM_POSITION, pilgrimPosition);
            MoveTo(pilgrimPosition);
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseBegin(ProtectPilgrimPhase phase)
        {
            pilgrimPosition = phase.pilgrimInstance.CurrentPosition;
        }

        void IPhaseListener<ProtectPilgrimPhase>.OnPhaseEnd(ProtectPilgrimPhase phase)
        {
            pilgrimPosition = Vector3.zero;
        }
    }
}
