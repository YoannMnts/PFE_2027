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
        
        public void PlayAttack()
        {
            attackRequestTime = Time.time;
            TryConsumeAttackRequest();
        }

        private void TryConsumeAttackRequest()
        {
            // Moveset and input buffer come from the character data
            PlayerData data = Player?.Data;
            if (data == null || data.BasicAttack == null)
                return;

            if (Time.time - attackRequestTime > data.AttackInputBuffer)
                return;

            AttackData attack;
            if (!hitboxRunner.IsRunning)
                attack = data.BasicAttack;
            
            else if (hitboxRunner.Has<Combo>(out var combo) 
                     && combo.NextAttack != null)
                attack = combo.NextAttack;
            
            else
                return; // Keep the request buffered until the combo window opens or it expires

            hitboxRunner.Begin(attack);
            attackRequestTime = float.NegativeInfinity;
        }

        // What the player's attacks do to what they touch: pushed away from the player, base strength
        private void OnAttackHit(in HitboxHit hit)
        {
            if (hit.target is not IRuntimeNpc npc)
                return;

            KnockbackRequest request = PushBackExtension.FromHit(mesh.transform.position, hit, mesh.forward);
            npc.PushBackWithDamage(hit.hitbox.Damage, request);
        }
    }
}