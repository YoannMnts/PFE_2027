using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Players;
using PFE.Gameplay.Scripts.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    public partial class RuntimeDefaultPlayer
    {
        [SerializeField, BoxGroup("Attack", true, false, -1f)] 
        private HitboxAnchors hitboxAnchors;

        [SerializeField, BoxGroup("Attack")]
        private LayerMask hitMask;

        private HitboxRunner hitboxRunner;
        private float attackRequestTime = float.NegativeInfinity;
        
        private void LateUpdate()
        {
            UpdateLocomotionAnimation();
            UpdateFacing();

            if (hitboxRunner == null)
                return;

            hitboxRunner.LateTick();
            TryConsumeAttackRequest();
        }
        
        public void PlayAttack()
        {
            attackRequestTime = Time.time;
            TryConsumeAttackRequest();
        }

        private void TryConsumeAttackRequest()
        {
            // An attack pressed during a dash stays buffered and starts when the dash ends
            if (IsDashing)
                return;

            // Moveset and input buffer come from the character data
            PlayerData data = Player?.Data;
            if (data == null || data.BasicAttack == null)
                return;

            if (Time.time - attackRequestTime > data.AttackInputBuffer)
                return;

            AttackData attack;
            if (!hitboxRunner.IsRunning)
                attack = data.BasicAttack;
            
            else if (hitboxRunner.Has(WindowFlags.Combo) 
                     && hitboxRunner.Current is AttackData attackData 
                     && attackData.Next != null)
                attack = attackData.Next;
            
            else
                return; // Keep the request buffered until the combo window opens or it expires

            hitboxRunner.Begin(attack);
            attackRequestTime = float.NegativeInfinity;
        }

        // What the player's attacks do to what they touch: pushed away from the player
        private void OnAttackHit(in HitboxHit hit)
        {
            if (hit.target is not IRuntimeNpc npc)
                return;

            Vector3 direction = PushBackExtension.GetPushBackDirection(mesh.transform.position, hit.TargetPosition,
                mesh.forward, hit.hitbox.PushBackMultiplier);
            npc.PushBackWithDamage(hit.hitbox.Damage, direction);
        }
    }
}