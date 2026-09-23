using System;
using PFE.Core;
using PFE.Core.DummyBoss;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public abstract class RuntimeBoss : MonoBehaviour, IRuntimeBoss
    {
        private BossInstance bossInstance;

        public void Setup(BossInstance boss)
        {
            bossInstance = boss;
        }

        [Button, DisableInEditorMode]
        public void Damage(int damage)
        {
            bossInstance.AddOrRemoveHealth();
        }
    }
}