using System.Collections.Generic;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases.Runtimes
{
    public abstract class RuntimePlayer : MonoBehaviour
    {
        public abstract void Disconnect();
    }

    public abstract class RuntimePlayer<T> : RuntimePlayer, IRuntimePlayer<T> where T : class, IPlayer
    {
        protected static readonly List<RuntimePlayer<T>> RuntimeBattlePlayers = new();
        
        public T Player { get; private set; }

        public void Connect(T player)
        {
            if (Player != null)
                Disconnect();

            Player = player;
            OnConnected();
        }

        public sealed override void Disconnect()
        {
            if (Player != null)
            {
                OnDisconnected();
                Player = null;
            }
        }

        protected abstract void OnConnected();
        protected abstract void OnDisconnected();
        
        public static bool TryGetRuntimePlayerFor<TRuntimePlayer>(T player, out TRuntimePlayer runtimePlayer)
            where TRuntimePlayer : RuntimePlayer<T>
        {
            foreach (RuntimePlayer<T> runtimeBattlePlayer in RuntimeBattlePlayers)
                if (runtimeBattlePlayer.Player == player && runtimeBattlePlayer is TRuntimePlayer rtp)
                {
                    runtimePlayer = rtp;
                    return true;
                }

            runtimePlayer = null;
            return false;
        }
    }
}