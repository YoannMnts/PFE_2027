using PFE.Core.Scripts.Fight.Dashes;
using PFE.Core.Scripts.Players;
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
            PlayerData data = Player?.Data;
            DashData dash = data != null ? data.Dash : null;
            if (dash == null || hitboxRunner == null || IsDashing || Time.time < dashReadyTime)
                return;

            // The CharacterMotor only drives the velocity on the ground
            if (characterMotor == null || !characterMotor.IsGrounded)
                return;

            // Replaces the current attack; the cooldown counts from the end of the dash
            hitboxRunner.Begin(dash);
            dashReadyTime = Time.time + dash.PlaySeconds + dash.Cooldown;
        }
    }
}
