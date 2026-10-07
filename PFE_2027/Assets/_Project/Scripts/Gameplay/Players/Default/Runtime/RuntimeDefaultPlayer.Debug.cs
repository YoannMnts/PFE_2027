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
            if (pilgrim == null)
                return;

            UnityEditor.Handles.color = new Color(1f,  0f, 0.07f, 1f);
            UnityEditor.Handles.DrawWireDisc(pilgrim.CurrentPosition, Vector3.up * 2, LeashRadius);
        }
#endif
    }
}