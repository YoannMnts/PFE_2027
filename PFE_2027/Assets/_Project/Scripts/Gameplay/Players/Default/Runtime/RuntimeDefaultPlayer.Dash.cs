using System;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Fight.Dashes;
using PFE.Core.Scripts.Players;
using PFE.Gameplay.Scripts.BulletTimes;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.NPCs;
using PFE.Utilities.Scripts.Timers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Dash: plays PlayerData.Dash through the HitboxRunner like an attack (animation, windows). The movement comes
    // from its Displacement window (RuntimeDefaultPlayer.Displacement), a Combo window can chain it into an attack.
    public partial class RuntimeDefaultPlayer
    {
        private float dashReadyTime;
        private Timer dashChargeTimer;
        private bool isInDashChain = false;
        
        public bool IsDashing => hitboxRunner != null && hitboxRunner.Current is DashData;

        public void StartCharging()
        {
            dashChargeTimer.Stop();
            dashChargeTimer.Run();
        }
        
        // Called by the input (CharacterControllerExtension)
        public void Dash()
        {
            //Douane  + check de la data (dash || dash chargé)
            PlayerData data = Player.Data;
            DashData currentDashData = dashChargeTimer.IsFinish || isInDashChain ? data.ChargeDash : data.DefaultDash;
            Debug.Log($"[Runtime player Dash] {currentDashData.name}");
            if (currentDashData == null || hitboxRunner == null || IsDashing || Time.time < dashReadyTime)
                return;
            
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
