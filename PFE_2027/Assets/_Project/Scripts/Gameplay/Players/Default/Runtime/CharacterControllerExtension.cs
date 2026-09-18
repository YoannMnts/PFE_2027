using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public class CharacterControllerExtension : MonoBehaviour
    {
        private RuntimeDefaultPlayer runtimePlayer;

        private void Awake()
        {
            runtimePlayer = GetComponentInParent<RuntimeDefaultPlayer>();
        }

        public void PerformAttack()
        {
            runtimePlayer.Player.CastAttack();
        }
    }
}
