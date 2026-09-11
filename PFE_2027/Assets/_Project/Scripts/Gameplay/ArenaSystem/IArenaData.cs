using PFE.Core.Scripts.DataMapping.Interfaces;

namespace PFE.Gameplay.Scripts.ArenaSystem
{
    public interface IArenaData : IData
    {
        public RuntimeArena ArenaPrefab { get; }
    }
}