using System;
using Helteix.Tools;
using PFE.Gameplay.Scripts.Players;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PFE.Gameplay.Scripts.Phases.Runtimes
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
            if (player is T)
            {
                runtimePlayer = SpawnPrefab();
                if (runtimePlayer is RuntimePlayer<T> compatible)
                {
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
