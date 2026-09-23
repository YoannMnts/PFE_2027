using System;
using PFE.Core;
using PFE.Core.DummyEnemy;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Phases
{
    public abstract class RuntimeEnemy : MonoBehaviour, IRuntimeEnemy
    {
        private EnemyInstance enemyInstance;

        public void Setup(EnemyInstance enemy)
        {
            enemyInstance = enemy;
        }

        [Button, DisableInEditorMode]
        public void Damage(int damage)
        {
            enemyInstance.AddOrRemoveHealth(damage);
        }
    }
}