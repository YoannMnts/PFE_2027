using Helteix.Tools.Phases;
using PFE.Core.Scripts.Area;

namespace PFE.Core.Scripts.GameModes
{
    public interface IGameMode : IPhase
    {
        
    }
    
    public abstract class GameMode<T> : Phase<T>, IGameMode
    {
        public CampfireAreaData CampfireAreaData { get; private set; }
    }
}