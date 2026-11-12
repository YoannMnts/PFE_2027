using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
#if UNITY_EDITOR
        [Button, DisableInEditorMode]
        public void DebugDamage(int damage)
        {
            TakeDamage(damage);
        }
        
        private void OnDrawGizmosSelected()
        {
        }
#endif
    }
}