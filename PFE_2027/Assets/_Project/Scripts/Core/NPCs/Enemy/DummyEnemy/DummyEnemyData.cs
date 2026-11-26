using UnityEngine;

namespace PFE.Core.Scripts.Enemy
{
    // Data of the DummyEnemy enemy: only what is specific to it (shared fields live in EnemyData / NpcData)
    [CreateAssetMenu(menuName = "PFE/Npcs/Enemies/Dummy Enemy", fileName = "DummyEnemy")]
    public class DummyEnemyData : EnemyData
    {
    }
}
