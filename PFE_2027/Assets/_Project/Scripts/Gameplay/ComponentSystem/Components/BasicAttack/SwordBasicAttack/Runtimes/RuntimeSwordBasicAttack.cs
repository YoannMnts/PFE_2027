using Helteix.Tools;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ComponentSystem
{
    public class RuntimeSwordBasicAttack : RuntimeComponent
    {
        public override void Setup()
        {
            EndDuration().ListenForExceptions();
        }

        protected override void PerformAttack()
        {
            transform.position += transform.forward * 0.2f;
        }

        private async Awaitable EndDuration()
        {
            await Awaitable.WaitForSecondsAsync(2f);
            Destroy(gameObject);
        }
    }
}