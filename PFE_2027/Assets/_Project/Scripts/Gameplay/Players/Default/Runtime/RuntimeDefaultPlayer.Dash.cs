using System;
using Helteix.ChanneledProperties.Priorities;
using Helteix.Tools.Phases;
using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Fight.Charges;
using PFE.Core.Scripts.Fight.Dashes;
using PFE.Gameplay.Scripts.BulletTimes;
using PFE.Gameplay.Scripts.Enemy.Runtime;
using PFE.Gameplay.Scripts.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Dash: plays its DashData through the AnimationRunner like an attack. The movement comes from its Displacement
    // flag (RuntimeDefaultPlayer.Displacement), a Combo flag can chain it into an attack.
    // Charge: the ChargeUp flag of the default dash gives its charge levels, played while the input is held; the dash
    // that comes out is the Release of the level playing at the release of the input.
    // Dash chain: a dash that hits an enemy starts the bullet time and puts the player straight into the charge level
    // of its ChargeOnHit flag: the next release brings out its Release.
    public partial class RuntimeDefaultPlayer
    {
        private float dashReadyTime;

        public bool IsDashing => animationRunner != null && animationRunner.Current is DashData;

        // Called by the input when the dash is pressed: starts the charge of the dash (ChargeUp flag).
        // Already charging (put in it by a dash hit): nothing to start, the release brings it out.
        public void StartCharging()
        {
            if (animationRunner == null || animationRunner.IsCharging || !CanStartDash())
                return;

            animationRunner.BeginCharge(Player.Data.DefaultDash);
        }

        // Called by the input when the dash is released: the Release of the charge level playing comes out
        public void Dash()
        {
            if (animationRunner == null || !animationRunner.TryReleaseCharge(out AnimationData released) || released == null)
                return;

            if (released is not DashData dash)
            {
                Debug.LogWarning($"[Runtime player Dash] Charge release '{released.name}' is not a DashData: no dash.", released);
                return;
            }

            EndBulletTime(true);

            // The CharacterMotor only drives the velocity on the ground
            if (characterMotor == null || !characterMotor.IsGrounded)
                return;

            Debug.Log($"[Runtime player Dash] {dash.name}");

            // Replaces the current action; the cooldown counts from the end of the dash
            animationRunner.Begin(dash, OnDashHit);
            dashReadyTime = Time.time + dash.PlaySeconds + dash.Cooldown;
        }

        // Conditions to start charging a dash. A dash hit bypasses them: it puts the player in the charge itself.
        private bool CanStartDash() => !IsDashing && Time.time >= dashReadyTime
            && characterMotor != null && characterMotor.IsGrounded;

        private void OnDashHit(in HitboxHit hit)
        {
            if (hit.target is not RuntimeEnemy npc)
                return;

            KnockbackRequest request = PushBackExtension.FromHit(mesh.transform.position, hit, mesh.forward);
            npc.PushBackWithDamage(hit.hitbox.Damage, request);

            // Dash chain: the dash is replaced by the charge level of its ChargeOnHit flag. Only once: for the next
            // enemies of the same hit, the current action is already that charge (no ChargeOnHit flag)
            AnimationData dash = animationRunner.Current;
            if (dash == null || !dash.TryGetFlag(out ChargeOnHit chargeOnHit) || chargeOnHit.Charge == null)
                return;

            if (currentBulletTimePhase == null)
            {
                currentBulletTimePhase = new BulletTimePhase();
                currentBulletTimePhase.RunAndForget();
            }

            animationRunner.BeginChargeAt(chargeOnHit.Charge);
        }
        
        private void EndBulletTime(bool byDash = false)
        {
            if (currentBulletTimePhase == null)
                return;

            currentBulletTimePhase.SetResult(byDash);
            currentBulletTimePhase = null;
        }
    }
}
