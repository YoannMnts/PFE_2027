using System;
using Helteix.Tools;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Runtime
{
    public interface IRuntimePlayerFactory
    {
        bool TryCreateRuntimeFor(IPlayer player, out RuntimePlayer runtimePlayer);
    }
    
    [Serializable]
    public abstract class RuntimePlayerFactory<T> : IRuntimePlayerFactory where T : class, IPlayer
    {
        [SerializeField]
        protected RuntimePlayer<T> runtimePlayerPrefab;

        public bool TryCreateRuntimeFor(IPlayer player, out RuntimePlayer runtimePlayer)
        {
            if (player is T t)
            {
                runtimePlayer = SpawnPrefab();
                if (runtimePlayer is RuntimePlayer<T> compatible)
                {
                    compatible.Connect(t);
                    return true;
                }
            }
            
            runtimePlayer = null;
            return false;
        }

        private RuntimePlayer<T> SpawnPrefab()
        {
            return runtimePlayerPrefab.InstantiatePrefab();
        }
    }
}
