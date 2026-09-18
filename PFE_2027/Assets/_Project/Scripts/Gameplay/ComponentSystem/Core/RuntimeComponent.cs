using System;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public abstract class RuntimeComponent : MonoBehaviour, IRuntimeComponent
    {
        public abstract void Setup();
        protected abstract void PerformAttack();
        private void FixedUpdate()
        {
            PerformAttack();
        }
    }
}