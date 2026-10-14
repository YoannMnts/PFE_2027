using PFE.Core.Scripts.Enemy;

namespace PFE.Gameplay.Scripts.Enemy
{
    // Rules of the DummyEnemy enemy (stateless, its state lives in EnemyInstance).
    // Override the Enemy<TData> defaults here: RedirectTargetPosition, ModifyHealth, Dying...
    public partial class DummyEnemy : Enemy<DummyEnemyData>
    {
        
    }
}
