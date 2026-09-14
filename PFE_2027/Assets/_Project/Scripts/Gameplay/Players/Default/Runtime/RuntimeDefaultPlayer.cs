using PFE.Gameplay.Scripts.Players.Runtime;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer : RuntimePlayer<DefaultPlayer>
    {
        [SerializeField]
        private Rigidbody rigidBody;
        
        protected override void OnConnected()
        {
            rigidBody.constraints = RigidbodyConstraints.FreezePositionY;
        }
        
        protected override void OnDisconnected()
        {
        }

    }
}
