using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.Enemy.Attacks.BasicAttacks;
using PFE.Core.Scripts.Fight.Dashes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PFE.Core.Scripts.Players
{
    // Stats of a playable character. The game mode gives it to the player when creating it,
    // GameMetricsSettings only references the default one.
    [CreateAssetMenu(menuName = "PFE/Player/Player Data")]
    public class PlayerData : GameDatabaseObject
    {
        [field: SerializeField, Min(1f), BoxGroup("Metrics")]
        public float MaxHealth { get; private set; } = 100f;
        
        [field: SerializeField, Min(1f), BoxGroup("Metrics")]
        public float FocusRadius { get; private set; } = 10;

        // Focus target choice: cost = angle part * FocusAngleWeight + distance part * FocusDistanceWeight (lowest wins)
        [field: SerializeField, Min(0f), BoxGroup("Metrics")]
        public float FocusAngleWeight { get; private set; } = 0.7f;

        [field: SerializeField, Min(0f), BoxGroup("Metrics")]
        public float FocusDistanceWeight { get; private set; } = 0.3f;

        // First attack of the combo chain (the next ones follow AttackData.Next)
        [field: SerializeField, BoxGroup("Attack")]
        public BasicAttackData BasicAttack { get; private set; }

        // How long (in seconds) an attack press stays valid while waiting for the combo window to open
        [field: SerializeField, Min(0f), BoxGroup("Attack")]
        public float AttackInputBuffer { get; private set; } = 0.25f;
        
        // Dash of this character: animation, windows (Displacement for the movement), duration and cooldown
        [field: SerializeField, BoxGroup("Dash")]
        public DashData DefaultDash { get; private set; }
        
        // charge up Dash of this character
        [field: SerializeField, BoxGroup("Dash")]
        public DashData ChargeDash { get; private set; }
        
        [field: SerializeField, BoxGroup("Dash"), Min(0f)]
        public float ChargeUpTime { get;  private set; }
        
        [field: SerializeField, BoxGroup("Dash")]
        public int MaxDashSequency { get; private set; }
    }
}
