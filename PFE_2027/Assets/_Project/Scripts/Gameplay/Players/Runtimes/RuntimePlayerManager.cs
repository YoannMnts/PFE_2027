using System;
using System.Collections.Generic;
using Helteix.Tools;
using Helteix.Tools.Phases.Listeners;
using PFE.Gameplay.Scripts.Players;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases.Runtimes
{
    public class RuntimePlayerManager : MonoPhaseListener<BattlePhase>
    {
        [Header("References"), SerializeField, ChildGameObjectsOnly]
        private Transform container;
        
        [SerializeReference]
        private IRuntimePlayerFactory[] factories;
        
        private Dictionary<IPlayer, RuntimePlayer> runtimeBattlePlayers;

        private void Awake()
        {
            runtimeBattlePlayers = new ();
        }

        protected override void OnPhaseBegin(BattlePhase phase)
        {
            base.OnPhaseBegin(phase);

            container.ClearChildren();

            Debug.Log($"Player count {phase.PlayerCount}");
            foreach (var player in phase.Players)
            {
                for (int i = 0; i < factories.Length; i++)
                {
                    var factory = factories[i];
                    if(!factory.TryCreateRuntimeFor(player, out var runtime))
                        continue;
                    
                    runtimeBattlePlayers.Add(player, runtime);
                    runtime.transform.SetParent(container);
                    break;
                }
            }
        }

        protected override void OnPhaseEnd(BattlePhase phase)
        {
            foreach ((_, RuntimePlayer runtime) in runtimeBattlePlayers)
                runtime.Disconnect();
            
            runtimeBattlePlayers.Clear();
            container.ClearChildren();
            
            base.OnPhaseEnd(phase);
        }
    }
}