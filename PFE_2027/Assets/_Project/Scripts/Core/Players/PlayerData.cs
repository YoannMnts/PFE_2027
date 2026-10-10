using PFE.Core.Scripts.Databases;
using PFE.Core.Scripts.Enemy.Attacks.BasicAttacks;
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

        // First attack of the combo chain (the next ones follow AttackData.Next)
        [field: SerializeField, BoxGroup("Attack")]
        public BasicAttackData BasicAttack { get; private set; }

        // How long (in seconds) an attack press stays valid while waiting for the combo window to open
        [field: SerializeField, Min(0f), BoxGroup("Attack")]
        public float AttackInputBuffer { get; private set; } = 0.25f;
        
        // X = time since the dash started (s), Y = distance travelled since the start (m).
        // Last key = total duration and total distance, slope = speed. Must start at (0, 0) and never go down.
        [field: SerializeField, BoxGroup("Dash")]
        public AnimationCurve DashDistanceInTime { get; private set; }

        // Time (in seconds) after the end of a dash before the next one is allowed
        [field: SerializeField, Min(0f), BoxGroup("Dash")]
        public float DashCooldown { get; private set; } = 0.4f;
    }
}
