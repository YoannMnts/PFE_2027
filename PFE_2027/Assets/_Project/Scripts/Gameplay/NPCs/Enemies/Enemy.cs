using PFE.Core.Scripts.Enemy;
using PFE.Core.Scripts.NPCs;
using UnityEngine;

namespace PFE.Gameplay.Scripts.Enemy
{
    public abstract class Enemy<TData> : IEnemy<TData> where TData : EnemyData
    {
        public bool CanSpawn(TData data)
        {
             return true;
        }

        public abstract void Attack(TData data);

        public float ModifyHealth(TData data, int damage, float currentHealth)
        {
            var modifyHealth = Mathf.Clamp(currentHealth + damage, 0, data.MaxHealth);
            return modifyHealth;
        }

        public void Dying(TData data)
        {
            Debug.Log("Dying");
            //le runtime fera remonter l'info quand il faudra mourir
            return;
        }
    }
}
