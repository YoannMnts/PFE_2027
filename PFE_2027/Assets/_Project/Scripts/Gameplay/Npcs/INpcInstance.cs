using System;
using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.NPCs
{
    // Non-generic handle so every instance can be stored together (NpcManager)
    public interface INpcInstance
    {
        public event Action<float> OnModifyHealth;
        public event Action<INpcData> OnDeath;

        public INpcData Data { get; }
        public float CurrentHealth { get; }
        public Vector3 Goal { get; }
        public Vector3 TargetPosition { get; }
        public void SetGoal(Vector3 goal);
        public void AddOrRemoveHealth(int amount);
        public bool IsDead { get; }

    }
}
