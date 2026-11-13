using PFE.Core.Scripts.Enemy.Attacks;
using PFE.Core.Scripts.Fight.Dashes;
using PFE.Core.Scripts.Players;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Players.Default.Runtime
{
    // Dash: plays PlayerData.Dash through the HitboxRunner like an attack (animation, windows). The movement comes
    // from its Displacement window (RuntimeDefaultPlayer.Displacement), a Combo window can chain it into an attack.
    public partial class RuntimeDefaultPlayer
    {
        private float dashReadyTime;

        public bool IsDashing => hitboxRunner != null && hitboxRunner.Current is DashData;

        // Called by the input (CharacterControllerExtension)
        public void Dash()
        {
            //Douane et association des DATAs
            PlayerData data = Player?.Data;
            DashData dash = data != null ? data.Dash : null;
            if (dash == null || hitboxRunner == null || IsDashing || Time.time < dashReadyTime)
                return;

            // The CharacterMotor only drives the velocity on the ground
            if (characterMotor == null || !characterMotor.IsGrounded)
                return;
            
            Debug.Log($"[Runtime player Dash] Player is dashing");
            //TODO Si chargeUpTime >= 1 ==> Dash Chargé
            //sinon dash esquive
            
            // Replaces the current attack; the cooldown counts from the end of the dash
            hitboxRunner.Begin(dash);
            dashReadyTime = Time.time + dash.PlaySeconds + dash.Cooldown;
        }
        
        private void OnDashHit(in HitboxHit hit)
        {
            Debug.Log($"[Runtime player Dash] {hit.collider.gameObject.name}");

            //TODO faire la logic de collision de dash chargé
        }
    }
}
