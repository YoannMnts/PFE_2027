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

        private AttackRunner attackRunner;
        private float attackRequestTime = float.NegativeInfinity;
        
        private void LateUpdate()
        {
            UpdateFacing();

            if (attackRunner == null)
                return;

            attackRunner.LateTick();
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
            if (!attackRunner.IsRunning)
                attack = data.BasicAttack;
            else if (attackRunner.Has(AttackFlags.Combo) && attackRunner.Current.Next != null)
                attack = attackRunner.Current.Next;
            else
                return; // Keep the request buffered until the combo window opens or it expires

            // Every hit (first one and combo ones) goes where the player is aiming
            SnapFacingToIntent();
            attackRunner.Begin(attack);
            attackRequestTime = float.NegativeInfinity;
        }

        // What the player's attacks do to what they touch
        private void OnAttackHit(in AttackHit hit)
        {
            if (hit.target is not IRuntimeNpc npc)
                return;

            Vector3 direction = PushBackExtension.GetPushBackDirection(mesh.forward, hit.hitbox.PushBackMultiplier);
            npc.PushBackWithDamage(hit.hitbox.Damage, direction);
        }
    }
}