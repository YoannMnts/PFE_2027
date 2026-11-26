using System;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Fight.Dashes;
using PFE.Core.Scripts.Players;
using PFE.Gameplay.Scripts.BulletTimes;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Dash: plays PlayerData.Dash through the HitboxRunner like an attack (animation, windows). The movement comes
    // from its Displacement window (RuntimeDefaultPlayer.Displacement), a Combo window can chain it into an attack.
    // Charge: the ChargeUp flag of the default dash gives its charge levels, played by the HitboxRunner while held.
    public partial class RuntimeDefaultPlayer
    {
        private float dashReadyTime;
        private bool isInDashChain = false;

        public bool IsDashing => hitboxRunner != null && hitboxRunner.Current is DashData;

        // Called by the input when the dash is pressed: starts the charge of the dash, if it has one (ChargeUp flag)
        public void StartCharging()
        {
            // Right after a dash that hit, the next one comes out charged without charging (dash chain)
            if (isInDashChain || !CanStartDash())
                return;

            hitboxRunner.TryBeginCharge(Player.Data.DefaultDash);
        }

        // Called by the input when the dash is released
        public void Dash()
        {
            PlayerData data = Player.Data;

            // Release of the charge level playing, null if no charge ran (or that level has no Release)
            HitboxData released = hitboxRunner?.ReleaseCharge();

            DashData currentDashData;
            if (isInDashChain)
            {
                currentDashData = data.ChargeDash;
            }
            else if (released is DashData chargedDash)
            {
                currentDashData = chargedDash;
            }
            else
            {
                if (released != null)
                    Debug.LogWarning($"[Runtime player Dash] Charge release '{released.name}' is not a DashData, default dash used.", released);

                currentDashData = data.DefaultDash;
            }

            if (currentDashData == null || hitboxRunner == null || IsDashing || Time.time < dashReadyTime)
                return;

            Debug.Log($"[Runtime player Dash] {currentDashData.name}");
            
            if (currentBulletTimePhase != null)
            {
                currentBulletTimePhase.SetResult(true);
            }

            // The CharacterMotor only drives the velocity on the ground
            if (characterMotor == null || !characterMotor.IsGrounded)
                return;
            Debug.Log($"[Runtime player Dash] Player is dashing");
            
            // Replaces the current attack; the cooldown counts from the end of the dash
            hitboxRunner.Begin(currentDashData, OnDashHit);
            dashReadyTime = Time.time + currentDashData.PlaySeconds + currentDashData.Cooldown;
        }

        // Same conditions as the dash itself: no charge that could never come out
        private bool CanStartDash() => hitboxRunner != null && !IsDashing && Time.time >= dashReadyTime
            && characterMotor != null && characterMotor.IsGrounded;
        
        private void OnDashHit(in HitboxHit hit)
        {
            if (hit.target is not RuntimeEnemy npc)
                return;
            
            currentBulletTimePhase = new BulletTimePhase();
            currentBulletTimePhase.RunAndForget();
            isInDashChain = true;
            
            KnockbackRequest request = PushBackExtension.FromHit(mesh.transform.position, hit, mesh.forward);
            npc.PushBackWithDamage(hit.hitbox.Damage, request);
        }
        
    }
}
