using PFE.Core.Scripts.Databases;
using UnityEngine;

namespace PFE.Gameplay.Scripts.ArenaSystem
{
    [CreateAssetMenu(fileName = "ArenaData", menuName = "PFE/ArenaData")]
    public class ArenaData : GameDatabaseObject, IArenaData
    {
        [field: SerializeField]
        public RuntimeArena ArenaPrefab { get; private set; }
    }
}